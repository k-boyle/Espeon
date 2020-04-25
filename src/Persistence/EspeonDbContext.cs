using Disqord;
using Disqord.Bot.Prefixes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonDbContext : DbContext {
#if DEBUG        
        private readonly DbContextOptions _options;
#endif        
        private readonly Config _config;
        private readonly ILogger _logger;
        private const string MentionPrefixLiteral = "<mention>";
        
        private DbSet<GuildPrefixes> GuildPrefixes { get; set; }
        private DbSet<UserLocalisation> UserLocalisations { get; set; }
        private DbSet<UserReminder> UserReminders { get; set; }
        private DbSet<Tag> Tags { get; set; }
        
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
            var dtoConverter = new DateTimeOffsetToBinaryConverter();
            
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
                    model.Property(localisation => localisation.Value)
                        .HasConversion(new EnumToNumberConverter<Localisation, int>());
                });

            modelBuilder.Entity<UserReminder>(
                model => {
                    model.HasKey(reminder => reminder.Id);
                    model.Property(reminder => reminder.Id).ValueGeneratedOnAdd();
                    model.Property(reminder => reminder.Value).ValueGeneratedNever();
                    model.Property(reminder => reminder.ChannelId).ValueGeneratedNever();
                    model.Property(reminder => reminder.TriggerAt).ValueGeneratedNever();
                    model.Property(reminder => reminder.UserId).ValueGeneratedNever();
                    model.Property(reminder => reminder.ReminderMessageId).ValueGeneratedNever();
                    model.Property(reminder => reminder.TriggerAt).HasConversion(dtoConverter);
                });

            modelBuilder.Entity<Tag>(
                model => {
                    model.HasKey(tag => tag.Id);
                    model.Property(tag => tag.Id).ValueGeneratedOnAdd();
                    model.Property(tag => tag.Key).ValueGeneratedNever();
                    model.Property(tag => tag.Uses).ValueGeneratedNever();
                    model.Property(tag => tag.Value).ValueGeneratedNever();
                    model.Property(tag => tag.CreateAt).ValueGeneratedNever();
                    model.Property(tag => tag.CreateAt).HasConversion(dtoConverter);
                });

            modelBuilder.Entity<GuildTag>(
                model => {
                    model.Property(tag => tag.CreatorId).ValueGeneratedNever();
                    model.Property(tag => tag.GuildId).ValueGeneratedNever();
                    model.Property(tag => tag.OwnerId).ValueGeneratedNever();
                });
        }

        private IPrefix ParseStringAsPrefix(string value) {
            return string.Equals(value, MentionPrefixLiteral, StringComparison.OrdinalIgnoreCase) 
                ? MentionPrefix.Instance as IPrefix
                : new StringPrefix(value);
        }
    }
}