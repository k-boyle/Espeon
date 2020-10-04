using Disqord.Bot.Prefixes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Espeon {
    public partial class EspeonDbContext : DbContext {
        private const string MentionPrefixLiteral = "<mention>";
        
        private readonly ILogger _logger;

        public DbSet<GuildPrefixes> GuildPrefixes { get; set; }
        public DbSet<UserLocalisation> UserLocalisations { get; set; }
        public DbSet<UserReminder> UserReminders { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<GuildTags> GuildTags { get; set; }

        public EspeonDbContext(DbContextOptions options, ILogger logger) : base(options) {
            this._logger = logger.ForContext("SourceContext", nameof(EspeonDbContext));
        }
        
        internal EspeonDbContext(DbContextOptions options) : base(options) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            var dtoConverter = new DateTimeOffsetToBinaryConverter();
            
            modelBuilder.Entity<GuildPrefixes>(
                model => {
                    model.HasIndex(prefixes => prefixes.GuildId).IsUnique();
                    model.Property(prefixes => prefixes.GuildId).ValueGeneratedNever();
                    if (Database.IsNpgsql()) {
                        model.Property(prefixes => prefixes.Values).HasConversion(
                            prefixes => prefixes.Select(x => x.ToString()).ToArray(),
                            arr => new HashSet<IPrefix>(arr.Select(ParseStringAsPrefix)));
                    } else {
                        model.Ignore(prefixes => prefixes.Values);
                    }
                });

            modelBuilder.Entity<UserLocalisation>(
                model => {
                    model.HasKey(
                        localisation => new {
                            localisation.GuildId,
                            localisation.UserId
                        });
                    model.Property(localisation => localisation.GuildId).ValueGeneratedNever();
                    model.Property(localisation => localisation.UserId).ValueGeneratedNever();
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

            modelBuilder.Entity<GuildTags>(
                model => {
                    model.HasIndex(tags => tags.GuildId).IsUnique();
                    model.Property(tags => tags.GuildId).ValueGeneratedNever();
                    model.HasMany(tags => tags.Values).WithOne(tag => tag.GuildTags).OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity<GuildTag>(
                model => {
                    model.HasOne(tag => tag.GuildTags).WithMany(tags => tags.Values).HasForeignKey(tag => tag.GuildId);
                    model.Property(tag => tag.GuildId).ValueGeneratedNever();
                    model.Property(tag => tag.CreatorId).ValueGeneratedNever();
                    model.Property(tag => tag.OwnerId).ValueGeneratedNever();
                });

            //ef core won't properly discriminate subclasses without this existing
            modelBuilder.Entity<GlobalTag>(model => { });
        }

        private static IPrefix ParseStringAsPrefix(string value) {
            return string.Equals(value, MentionPrefixLiteral, StringComparison.OrdinalIgnoreCase) 
                ? MentionPrefix.Instance as IPrefix
                : new StringPrefix(value);
        }
    }
}