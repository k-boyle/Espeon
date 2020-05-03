using Disqord;
using Disqord.Bot.Prefixes;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon {
    public class PrefixService {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<ulong, GuildPrefixes> _guildPrefixes;

        public PrefixService(IServiceProvider services, ILogger logger) {
            this._services = services;
            this._logger = logger.ForContext("SourceContext", nameof(PrefixService));
            this._guildPrefixes = new ConcurrentDictionary<ulong, GuildPrefixes>();
        }
        
        public ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(IGuild guild) {
            return this._guildPrefixes.TryGetValue(guild.Id, out var prefixes) 
                ? new ValueTask<IEnumerable<IPrefix>>(prefixes.Values)
                : new ValueTask<IEnumerable<IPrefix>>(GetPrefixesFromDbAsync(guild));
        }
        
        private async Task<IEnumerable<IPrefix>> GetPrefixesFromDbAsync(IGuild guild) {
            this._logger.Information("Loading prefixes from db for {Guild}", guild.Name);
            using var scope = this._services.CreateScope();
            await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();
            return (this._guildPrefixes[guild.Id] = await context.GetPrefixesAsync(guild)).Values;
        }

        public ValueTask<bool> TryAddPrefixAsync(IGuild guild, IPrefix prefix) {
            this._logger.Information("Adding prefix {Prefix} to {Guild}", prefix, guild.Name);
            return TryModifyAsync(guild, prefix, (prefixes, prefix) => prefixes.Values.Add(prefix));
        }

        public ValueTask<bool> TryRemovePrefixAsync(IGuild guild, IPrefix prefix) {
            this._logger.Information("Removing prefix {Prefix} to {Guild}", prefix, guild.Name);
            return TryModifyAsync(guild, prefix, (prefixes, prefix) => prefixes.Values.Remove(prefix));
        }
        
        private ValueTask<bool> TryModifyAsync(IGuild guild, IPrefix prefix, Func<GuildPrefixes, IPrefix, bool> modifyFunc) {
            var modified = false;
            var prefixes = this._guildPrefixes.AddOrUpdate(
                guild.Id,
                (id, func) => throw new KeyNotFoundException($"{guild.Id.ToString()} was not found in prefix cache, this shouldn't happen"),
                (id, prefixes, func) => {
                    modified = func(prefixes, prefix);
                    return prefixes;
                },
                modifyFunc);
            
            return modified
                ? new ValueTask<bool>(PersistAsync(prefixes))
                : new ValueTask<bool>(false);
        }
        
        private async Task<bool> PersistAsync(GuildPrefixes prefixes) {
            using var scope = this._services.CreateScope();
            await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();
            await context.UpdateAsync(prefixes);
            return true;
        }
    }
}