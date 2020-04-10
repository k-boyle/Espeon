using Disqord;
using Disqord.Bot.Prefixes;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Persistence {
    public class EspeonDbContext : DbContext {
        private readonly Config _config;
        private readonly ILogger _logger;
        private const string MentionPrefixLiteral = "<mention>";
        
        public EspeonDbContext(Config config, ILogger logger) {
            this._config = config;
            this._logger = logger.ForContext("SourceContext", GetType().Name);
        }
        
        private DbSet<GuildPrefixes> GuildPrefixes { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
            => optionsBuilder.UseNpgsql(this._config.Postgres.ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<GuildPrefixes>(model => {
                model.HasIndex(prefixes => prefixes.GuildId).IsUnique();
                model.Property(prefixes => prefixes.GuildId).ValueGeneratedNever();
                model.Property(prefixes => prefixes.Values).HasConversion(
                    prefixes => prefixes.Select(x => x.ToString()).ToArray(),
                    arr => new HashSet<IPrefix>(arr.Select(ParseStringAsPrefix)));
            });
        }

        public async Task<GuildPrefixes> GetPrefixesAsync(IGuild guild) {
            this._logger.Debug("Loading prefixes for guild {Guild}", guild.Name);
            return await GuildPrefixes.SingleOrDefaultAsync(prefix => prefix.GuildId == guild.Id);
        }

        public async Task PersistGuildAsync(IGuild guild) {
            this._logger.Debug("Persisting {Guild}", guild.Name);
            if (await GuildPrefixes.FindAsync(guild.Id.RawValue) != null) {
                return;
            }
            
            await GuildPrefixes.AddAsync(new GuildPrefixes(guild.Id));
            await SaveChangesAsync();
        }
        
        public async Task RemoveGuildAsync(IGuild guild) {
            this._logger.Debug("Removing {Guild}", guild.Name);
            var prefixes = await GuildPrefixes.FindAsync(guild.Id.RawValue);
            GuildPrefixes.Remove(prefixes);
            
            await SaveChangesAsync();
        }
        
        public async Task UpdateAsync<T>(T newData) where T : class {
            this._logger.Debug("Updating {@Data}", newData);
            switch (newData) {
                case GuildPrefixes prefixes:
                    GuildPrefixes.Update(prefixes);
                    break;
            }
            
            await SaveChangesAsync();
        }
        
        private IPrefix ParseStringAsPrefix(string value) {
            return string.Equals(value, MentionPrefixLiteral, StringComparison.OrdinalIgnoreCase) 
                ? MentionPrefix.Instance as IPrefix
                : new StringPrefix(value);
        }
    }
}