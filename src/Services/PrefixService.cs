using Disqord;
using Disqord.Bot.Prefixes;
using Espeon.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Services {
    public class PrefixService {
        private readonly IServiceProvider _services;
        private readonly Dictionary<ulong, GuildPrefixes> _guildPrefixes;
        private readonly object _lock = new object();

        public PrefixService(IServiceProvider services) {
            this._services = services;
            this._guildPrefixes = new Dictionary<ulong, GuildPrefixes>();
        }
        
        public ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(IGuild guild) {
            lock (this._lock) {
                return this._guildPrefixes.TryGetValue(guild.Id, out var prefixes) 
                    ? new ValueTask<IEnumerable<IPrefix>>(prefixes.Values)
                    : new ValueTask<IEnumerable<IPrefix>>(GetPrefixesFromDbAsync(guild));
            }
        }
        
        private async Task<IEnumerable<IPrefix>> GetPrefixesFromDbAsync(IGuild guild) {
            await using var context = this._services.GetService<EspeonDbContext>();
            var prefixes = await context.GetPrefixesAsync(guild);
            lock (this._lock) {
                return (this._guildPrefixes[guild.Id] = prefixes).Values;
            }
        }
        
        public ValueTask<bool> TryAddPrefixAsync(IGuild guild, IPrefix prefix) {
            lock (this._lock) {
                if (!this._guildPrefixes.TryGetValue(guild.Id, out var prefixes)) {
                    throw new KeyNotFoundException($"{guild.Id.ToString()} was not found in prefix cache, this shouldn't happen");
                }
                
                return prefixes.Values.Add(prefix)
                    ? new ValueTask<bool>(PersistAsync(prefixes))
                    : new ValueTask<bool>(false);
            }
        }
        
        public ValueTask<bool> TryRemovePrefixAsync(IGuild guild, IPrefix prefix) {
            lock (this._lock) {
                if (!this._guildPrefixes.TryGetValue(guild.Id, out var prefixes)) {
                    throw new KeyNotFoundException($"{guild.Id.ToString()} was not found in prefix cache, this shouldn't happen");
                }
                
                return prefixes.Values.Remove(prefix)
                    ? new ValueTask<bool>(PersistAsync(prefixes))
                    : new ValueTask<bool>(false);
            }
        }
        
        private async Task<bool> PersistAsync(GuildPrefixes prefixes) {
            await using var context = this._services.GetService<EspeonDbContext>();
            await context.UpdateAsync(prefixes);
            return true;
        }
    }
}