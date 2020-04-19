using Disqord;
using Disqord.Bot.Prefixes;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon {
    public class EspeonDbContext : DbContext {
#if DEBUG        
        private readonly DbContextOptions _options;
#endif        
        private readonly Config _config;
        private readonly ILogger _logger;
        private const string MentionPrefixLiteral = "<mention>";
        
        private DbSet<GuildPrefixes> GuildPrefixes { get; set; }
        private DbSet<UserLocalisation> UserLocalisations { get; set; }
        private DbSet<UserReminder> UserReminders { get; set; }
        
        public EspeonDbContext(Config config, ILogger logger) {
            this._config = config;
            this._logger = logger.ForContext("SourceContext", typeof(EspeonDbContext).Name);
        }

#if DEBUG
        public EspeonDbContext(DbContextOptions options) : base(options) {
            this._options = options;
        }
#endif

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
#if DEBUG            
            if (this._options != null) {
                return;
            }
#endif
            
            optionsBuilder.UseNpgsql(this._config.Postgres.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<GuildPrefixes>(
                model => {
                    model.HasIndex(prefixes => prefixes.GuildId).IsUnique();
                    model.Property(prefixes => prefixes.GuildId).ValueGeneratedNever();
                    model.Property(prefixes => prefixes.Values).HasConversion(
                        prefixes => prefixes.Select(x => x.ToString()).ToArray(),
                        arr => new HashSet<IPrefix>(arr.Select(ParseStringAsPrefix)));
                });

            modelBuilder.Entity<UserLocalisation>(
                model => {
                    model.HasKey(
                        localisation => new {
                            localisation.GuildId,
                            localisation.UserId
                        });
                    model.Property(localisation => localisation.GuildId).ValueGeneratedNever();
                    model.Property(locatisation => locatisation.UserId).ValueGeneratedNever();
                    model.Property(localisation => localisation.Value).HasConversion(
                        value => (int) value,
                        value => (Localisation) value);
                });

            modelBuilder.Entity<UserReminder>(
                model => {
                    model.HasKey(remidner => remidner.Id);
                    model.Property(reminder => reminder.Id).ValueGeneratedOnAdd();
                    model.Property(reminder => reminder.Value).ValueGeneratedNever();
                    model.Property(reminder => reminder.ChannelId).ValueGeneratedNever();
                    model.Property(reminder => reminder.TriggerAt).ValueGeneratedNever();
                    model.Property(reminder => reminder.UserId).ValueGeneratedNever();
                    model.Property(reminder => reminder.ReminderMessageId).ValueGeneratedNever();
                    model.Property(reminder => reminder.TriggerAt).HasConversion(
                        dateTime => dateTime.ToUnixTimeMilliseconds(),
                        unixTime => DateTimeOffset.FromUnixTimeMilliseconds(unixTime));
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
        
        public async Task<UserLocalisation> GetLocalisationAsync(IGuild guild, IUser user) {
            this._logger.Debug("Loading localisation for user {User}", user.Id);
            return await UserLocalisations.FindAsync(guild.Id.RawValue, user.Id.RawValue)
                 ?? await NewUserLocalisationAsync(guild, user);
        }
        
        private async Task<UserLocalisation> NewUserLocalisationAsync(IGuild guild, IUser user) {
            this._logger.Debug("Creating new user localisation for {User}", user.Id);
            var localisation = new UserLocalisation(guild.Id, user.Id);
            await UserLocalisations.AddAsync(localisation);
            await SaveChangesAsync();
            return localisation;
        }
        
        public async Task<IEnumerable<UserReminder>> GetRemindersAsync() {
            this._logger.Debug("Fetching all reminders");
            return await UserReminders.ToListAsync();
        }
        
        public async Task<IEnumerable<UserReminder>> GetRemindersAsync(Predicate<UserReminder> predicate) {
            this._logger.Debug("Fetching specific reminders");
            return (await UserReminders.ToListAsync()).Where(reminder => predicate(reminder));
        }
        
        public async Task PersistReminderAsync(UserReminder reminder) {
            this._logger.Debug("Persisting reminder for {User}", reminder.UserId);
            await UserReminders.AddAsync(reminder);
            await SaveChangesAsync();
        }
        
        public async Task RemoveReminderAync(UserReminder reminder) {
            this._logger.Debug("Removing reminder {Id}", reminder.Id);
            UserReminders.Remove(reminder);
            await SaveChangesAsync();
        }
        
        public async Task UpdateAsync<T>(T newData) where T : class {
            this._logger.Debug("Updating {@Data}", newData);
            switch (newData) {
                case GuildPrefixes prefixes:
                    GuildPrefixes.Update(prefixes);
                    break;
                
                case UserLocalisation localisation:
                    UserLocalisations.Update(localisation);
                    break;
                
                case UserReminder reminder:
                    UserReminders.Update(reminder);
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