using Discord;
using Espeon.Databases.Entities;
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
        public DbSet<CustomCommand> CustomCommands { get; set; }

        private static Config _config;

        public GuildStore(Config config)
            => _config = config;

        public GuildStore()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(_config.ConnectionStrings.GuildStore);

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

                guild.HasMany(x => x.Commands)
                    .WithOne(y => y.Guild)
                    .HasForeignKey(z => z.GuildId);
            });

            modelBuilder.Entity<CustomCommand>().HasKey(x => x.Id);
            modelBuilder.Entity<CustomCommand>().Property(x => x.Id)
                .ValueGeneratedOnAdd();
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
            var foundGuild = await Guilds.FindAsync(guild.Id)
                ?? await CreateGuildAsync<ulong>(guild, null); //kinda hacky?

            return foundGuild;
        }

        public async Task<Guild> GetOrCreateGuildAsync<TProp>(IGuild guild, Expression<Func<Guild, TProp>> expression)
        {
            var foundGuild = await Guilds.Include(expression).FirstOrDefaultAsync(x => x.Id == guild.Id)
                ?? await CreateGuildAsync(guild, expression);

            return foundGuild;
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
    }
}
