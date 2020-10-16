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
        private readonly ConcurrentDictionary<Language, ConcurrentDictionary<LocalisationStringKey, string>> _localisations;
        private readonly ConcurrentDictionary<(ulong, ulong), Language> _userLanguageCache;
        private readonly ConcurrentDictionary<string, LocalisationStringKey> _localisationCache;

        public LocalisationService(IServiceProvider services, ILocalisationProvider localisationProvider, ILogger logger) {
            this._services = services;
            this._localisationProvider = localisationProvider;
            this._logger = logger.ForContext("SourceContext", nameof(LocalisationService));
            this._localisations = new ConcurrentDictionary<Language, ConcurrentDictionary<LocalisationStringKey, string>>();
            this._userLanguageCache = new ConcurrentDictionary<(ulong, ulong), Language>();
            this._localisationCache = new ConcurrentDictionary<string, LocalisationStringKey>();
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            var sw = Stopwatch.StartNew();
            this._logger.Information("Loading all language strings");
            var localisations = await this._localisationProvider.GetLocalisationsAsync();
            foreach (var (language, localisationsStrings) in localisations) {
                this._localisations[language] = new ConcurrentDictionary<LocalisationStringKey, string>(localisationsStrings);
            }
            sw.Stop();
            this._logger.Information("All language strings loaded in {Time}ms", sw.ElapsedMilliseconds);
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
            return this._userLanguageCache.TryGetValue((guildId, memberId), out var language)
                ? new ValueTask<string>(GetResponse(language, stringKey, args))
                : new ValueTask<string>(GetUserLanguageFromDbAsync(memberId, guildId, stringKey, args));
        }

        private async Task<string> GetUserLanguageFromDbAsync(
                Snowflake memberId,
                Snowflake guildId,
                LocalisationStringKey stringKey,
                object[] args) {
            this._logger.Debug("Getting response string {key} for {user} from database", stringKey, memberId);
            using var scope = this._services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            var localisation = await context.GetLocalisationAsync(memberId, guildId);
            this._userLanguageCache[(guildId, memberId)] = localisation.Value;
            return GetResponse(localisation.Value, stringKey, args);
        }

        private string GetResponse(Language language, LocalisationStringKey stringKey, object[] args) {
            var unformattedString = this._localisations[language].GetValueOrDefault(stringKey,
                this._localisations[Language.Default][stringKey]);
            return args.Length > 0
                ? string.Format(unformattedString!, args)
                : unformattedString;
        }

        public LocalisationStringKey GetKey(string str) {
            return this._localisationCache.GetOrAdd(str, Enum.Parse<LocalisationStringKey>);
        }
    }
}