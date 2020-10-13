using Disqord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon {
    public class LocalisationService : IHostedService {
        private readonly IServiceProvider _services;
        private readonly ILocalisationProvider _localisationProvider;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<Localisation, ConcurrentDictionary<LocalisationStringKey, string>> _localisations;
        private readonly ConcurrentDictionary<(ulong, ulong), Localisation> _userLocalisationCache;
        private readonly ConcurrentDictionary<string, LocalisationStringKey> _localisationCache;

        public LocalisationService(IServiceProvider services, ILocalisationProvider localisationProvider, ILogger logger) {
            this._services = services;
            this._localisationProvider = localisationProvider;
            this._logger = logger.ForContext("SourceContext", nameof(LocalisationService));
            this._localisations = new ConcurrentDictionary<Localisation, ConcurrentDictionary<LocalisationStringKey, string>>();
            this._userLocalisationCache = new ConcurrentDictionary<(ulong, ulong), Localisation>();
            this._localisationCache = new ConcurrentDictionary<string, LocalisationStringKey>();
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            var sw = Stopwatch.StartNew();
            this._logger.Information("Loading all localisation strings");
            var localisations = await this._localisationProvider.GetLocalisationsAsync();
            foreach (var (localisationKey, localisationsStrings) in localisations) {
                this._localisations[localisationKey] = new ConcurrentDictionary<LocalisationStringKey, string>(localisationsStrings);
            }
            sw.Stop();
            this._logger.Information("All localisation strings loaded in {Time}ms", sw.ElapsedMilliseconds);
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        public ValueTask<string> GetResponseAsync(
                Snowflake memberId,
                Snowflake guildId,
                LocalisationStringKey stringKey,
                params object[] args) {
            this._logger.Debug("Getting response string {key} for {user}", stringKey, memberId);
            return this._userLocalisationCache.TryGetValue((guildId, memberId), out var localisation)
                ? new ValueTask<string>(GetResponse(localisation, stringKey, args))
                : new ValueTask<string>(GetUserLocalisationFromDbAsync(memberId, guildId, stringKey, args));
        }

        private async Task<string> GetUserLocalisationFromDbAsync(
                Snowflake memberId,
                Snowflake guildId,
                LocalisationStringKey stringKey,
                object[] args) {
            this._logger.Debug("Getting response string {key} for {user} from database", stringKey, memberId);
            using var scope = this._services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            var localisation = await context.GetLocalisationAsync(memberId, guildId);
            this._userLocalisationCache[(guildId, memberId)] = localisation.Value;
            return GetResponse(localisation.Value, stringKey, args);
        }

        private string GetResponse(Localisation localisation, LocalisationStringKey stringKey, object[] args) {
            var unformattedString = this._localisations[localisation].GetValueOrDefault(stringKey,
                this._localisations[Localisation.Default][stringKey]);
            return args.Length > 0
                ? string.Format(unformattedString!, args)
                : unformattedString;
        }

        public LocalisationStringKey GetKey(string str) {
            return this._localisationCache.GetOrAdd(str, Enum.Parse<LocalisationStringKey>);
        }
    }
}