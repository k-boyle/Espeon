using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Espeon.Databases.GuildStore
{
    public class GuildStore : DbContext
    {
        private DbSet<Guild> Guilds { get; set; }

        private static Config _config;

        public GuildStore(Config config)
            => _config = config;

        public GuildStore()
        {
        }

#if !DEBUG
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(_config.ConnectionStrings.GuildStore);
#else
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=GuildStore;Username=postgres;Password=casino");
#endif

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var snowflakeConverter = new SnowflakeCollectionParser();

            modelBuilder.Entity<Guild>(guild =>
            {
                guild.HasKey(x => x.Id);

                guild.Property(x => x.RestrictedChannels)
                    .HasConversion(snowflakeConverter);

                guild.Property(x => x.RestrictedUsers)
                    .HasConversion(snowflakeConverter);

                guild.Property(x => x.Admins)
                    .HasConversion(snowflakeConverter);

                guild.Property(x => x.Moderators)
                    .HasConversion(snowflakeConverter);

                guild.Property(x => x.SelfAssigningRoles)
                    .HasConversion(snowflakeConverter);

                guild.Property(x => x.WarningLimit)
                    .HasDefaultValue(3);

                guild.Property(x => x.StarLimit)
                    .HasDefaultValue(3);

                guild.HasMany(x => x.Commands)
                    .WithOne(y => y.Guild)
                    .HasForeignKey(z => z.GuildId);

                guild.HasMany(x => x.Warnings)
                    .WithOne(y => y.Guild)
                    .HasForeignKey(z => z.GuildId);

                guild.HasMany(x => x.StarredMessages)
                    .WithOne(y => y.Guild)
                    .HasForeignKey(z => z.GuildId);

                guild.Property(x => x.EmotesEnabled)
                    .HasDefaultValue(true);

                guild.Property(x => x.AutoQuotes)
                    .HasDefaultValue(false);

                guild.HasMany(x => x.Webhooks)
                    .WithOne(y => y.Guild)
                    .HasForeignKey(z => z.GuildId);
            });

            modelBuilder.Entity<CustomCommand>(command =>
            {
                command.HasKey(x => x.Id);

                command.Property(x => x.Id)
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Warning>(warning =>
            {
                warning.HasKey(x => x.Id);

                warning.Property(x => x.Id)
                    .ValueGeneratedOnAdd();

                warning.Property(x => x.IssuedOn)
                    .HasDefaultValue(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            });

            modelBuilder.Entity<StarredMessage>(message =>
            {
                message.HasKey(x => x.Id);

                message.Property(x => x.ReactionUsers)
                    .HasConversion(snowflakeConverter);
            });

            modelBuilder.Entity<MockWebhook>(webhook => webhook.HasKey(x => x.Id));
        }

        private sealed class SnowflakeCollectionParser : ValueConverter<ICollection<ulong>, string>
        {
            public SnowflakeCollectionParser(ConverterMappingHints mappingHints = null)
                : base(InExpression, OutExpression, mappingHints)
            { }

            private static readonly Expression<Func<ICollection<ulong>, string>> InExpression = collection
                => collection != null && collection.Any()
                    ? string.Join(',', collection)
                    : null;

            private static readonly Expression<Func<string, ICollection<ulong>>> OutExpression = str
                => !string.IsNullOrWhiteSpace(str)
                    ? str.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse).ToList()
                    : new List<ulong>();
        }

        public async Task<Guild> GetOrCreateGuildAsync(IGuild guild)
        {
            //kinda hacky?
            return await Guilds.FindAsync(guild.Id)
                ?? await CreateGuildAsync<ulong>(guild, null);
        }

        public async Task<Guild> GetOrCreateGuildAsync<TProp>(IGuild guild, Expression<Func<Guild, TProp>> expression)
        {
            return await Guilds.Include(expression).FirstOrDefaultAsync(x => x.Id == guild.Id)
                ?? await CreateGuildAsync(guild, expression);
        }

        private async Task<Guild> CreateGuildAsync<TProp>(IGuild guild, Expression<Func<Guild, TProp>> expression)
        {
            var newGuild = new Guild
            {
                Id = guild.Id,
                Prefixes = new List<string>
                {
                    "es/"
                },
                Admins = new List<ulong>
                {
                    guild.OwnerId
                },
                //lazy hack
                Moderators = new List<ulong>
                {
                    0
                },
                RestrictedUsers = new List<ulong>
                {
                    0
                },
                RestrictedChannels = new List<ulong>
                {
                    0
                },
                SelfAssigningRoles = new List<ulong>
                {
                    0
                }
            };

            await Guilds.AddAsync(newGuild);

            await SaveChangesAsync();

            if (expression is null)
                return await Guilds.FindAsync(guild.Id);

            return await Guilds.Include(expression).FirstOrDefaultAsync(x => x.Id == guild.Id);
        }

        public Task<IReadOnlyCollection<Guild>> GetAllGuildsAsync()
            => GetAllGuildsAsync<ulong>(null);

        public async Task<IReadOnlyCollection<Guild>> GetAllGuildsAsync<TProp>(Expression<Func<Guild, TProp>> expression)
        {
            if (expression is null)
            {
                return await Guilds.ToListAsync();
            }

            return await Guilds.Include(expression).ToListAsync();
        }

        public async Task RemoveGuildAsync(IGuild guild)
        {
            var dbGuild = await GetOrCreateGuildAsync(guild);
            Guilds.Remove(dbGuild);
        }
    }
}
