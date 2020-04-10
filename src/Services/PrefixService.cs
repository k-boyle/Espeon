using Disqord;
using Disqord.Bot.Prefixes;
using Espeon.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Services {
    public class PrefixService {
        private readonly IServiceProvider _services;
        private readonly ConcurrentDictionary<ulong, GuildPrefixes> _guildPrefixes;

        public PrefixService(IServiceProvider services) {
            this._services = services;
            this._guildPrefixes = new ConcurrentDictionary<ulong, GuildPrefixes>();
        }
        
        public ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(IGuild guild) {
            return this._guildPrefixes.TryGetValue(guild.Id, out var prefixes) 
                ? new ValueTask<IEnumerable<IPrefix>>(prefixes.Values)
                : new ValueTask<IEnumerable<IPrefix>>(GetPrefixesFromDbAsync(guild));
        }
        
        private async Task<IEnumerable<IPrefix>> GetPrefixesFromDbAsync(IGuild guild) {
            await using var context = this._services.GetService<EspeonDbContext>();
            return (this._guildPrefixes[guild.Id] = await context.GetPrefixesAsync(guild)).Values;
        }

        public ValueTask<bool> TryAddPrefixAsync(IGuild guild, IPrefix prefix) {
            var modified = false;
            var prefixes = this._guildPrefixes.AddOrUpdate(
                guild.Id,
                id => throw new KeyNotFoundException($"{guild.Id.ToString()} was not found in prefix cache, this shouldn't happen"),
                (id, prefixes) => {
                    modified = prefixes.Values.Add(prefix);
                    return prefixes;
                });
            
            return modified
                ? new ValueTask<bool>(PersistAsync(prefixes))
                : new ValueTask<bool>(false);
        }

        public ValueTask<bool> TryRemovePrefixAsync(IGuild guild, IPrefix prefix) {
            var modified = false;
            var prefixes = this._guildPrefixes.AddOrUpdate(
                guild.Id,
                id => throw new KeyNotFoundException($"{guild.Id.ToString()} was not found in prefix cache, this shouldn't happen"),
                (id, prefixes) => {
                    modified = prefixes.Values.Remove(prefix);
                    return prefixes;
                });
            
            return modified
                ? new ValueTask<bool>(PersistAsync(prefixes))
                : new ValueTask<bool>(false);
        }
        
        private async Task<bool> PersistAsync(GuildPrefixes prefixes) {
            await using var context = this._services.GetService<EspeonDbContext>();
            await context.UpdateAsync(prefixes);
            return true;
        }
    }
}