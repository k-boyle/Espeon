using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Espeon {
    public class LocalisationService : IHostedService {
        private readonly IServiceProvider _services;
        private readonly ILocalisationProvider _localisationProvider;
        private readonly ILogger<LocalisationService> _logger;
        private readonly ConcurrentDictionary<Language, ConcurrentDictionary<LocalisationStringKey, string>> _localisations;
        private readonly ConcurrentDictionary<(ulong, ulong), Language> _userLanguageCache;
        private readonly ConcurrentDictionary<string, LocalisationStringKey> _localisationCache;

        public LocalisationService(
                IServiceProvider services,
                ILocalisationProvider localisationProvider,
                ILogger<LocalisationService> logger) {
            this._services = services;
            this._localisationProvider = localisationProvider;
            this._logger = logger;
            this._localisations = new ConcurrentDictionary<Language, ConcurrentDictionary<LocalisationStringKey, string>>();
            this._userLanguageCache = new ConcurrentDictionary<(ulong, ulong), Language>();
            this._localisationCache = new ConcurrentDictionary<string, LocalisationStringKey>();
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            var sw = Stopwatch.StartNew();
            this._logger.LogInformation("Loading all language strings");
            var localisations = await this._localisationProvider.GetLocalisationsAsync();
            foreach (var (language, localisationsStrings) in localisations) {
                this._localisations[language] = new ConcurrentDictionary<LocalisationStringKey, string>(localisationsStrings);
            }
            sw.Stop();
            this._logger.LogInformation("All language strings loaded in {Time}ms", sw.ElapsedMilliseconds);
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        public ValueTask<string> GetResponseAsync(
                Snowflake memberId,
                Snowflake guildId,
                LocalisationStringKey stringKey,
                params object[] args) {
            this._logger.LogDebug("Getting response string {key} for {user}", stringKey, memberId);
            return this._userLanguageCache.TryGetValue((guildId, memberId), out var language)
                ? new ValueTask<string>(GetResponse(language, stringKey, args))
                : new ValueTask<string>(GetUserLanguageFromDbAsync(guildId, memberId, stringKey, args));
        }

        private async Task<string> GetUserLanguageFromDbAsync(
                Snowflake guildId,
                Snowflake memberId,
                LocalisationStringKey stringKey,
                object[] args) {
            this._logger.LogDebug("Getting response string {key} for {user} from database", stringKey, memberId);
            using var scope = this._services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();

            var localisation = await context.GetOrCreateAsync(
                guildId.RawValue,
                memberId.RawValue,
                (guildId, userId) => new UserLocalisation(guildId, userId));
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

        public async Task UpdateLocalisationAsync(
                EspeonDbContext context,
                ulong guildId,
                ulong userId,
                Language language) {
            var locale = await context.GetOrCreateAsync(
                guildId,
                userId,
                (guild, user) => new UserLocalisation(guild, user)
            );
            locale.Value = language;

            this._userLanguageCache[(guildId, userId)] = language;
            
            await context.UpdateAsync(locale);
        }
    }
}