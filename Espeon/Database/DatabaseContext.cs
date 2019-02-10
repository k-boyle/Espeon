using Discord;
using Espeon.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Espeon.Database
{
    public class DatabaseContext : DbContext
    {
        private DbSet<Guild> Guilds { get; set; }
        private DbSet<User> Users { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<ModuleInfo> Modules { get; set; }
        public DbSet<CommandInfo> Commands { get; set; }
        public DbSet<CustomCommand> CustomCommands { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseNpgsql($"Host=127.0.0.1;Port=5432;Database=postgres;Username=postgres;Password=casino");

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

            modelBuilder.Entity<User>(user =>
            {
                user.HasKey(x => x.Id);

                user.Property(x => x.CandyAmount)
                    .HasDefaultValue(10);

                user.Property(x => x.HighestCandies)
                    .HasDefaultValue(10);

                user.Property(x => x.ResponsePack)
                    .HasDefaultValue("default");

                user.HasMany(x => x.Reminders)
                    .WithOne();
            });

            modelBuilder.Entity<Reminder>().HasKey(x => x.Id);

            modelBuilder.Entity<Reminder>().Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<ModuleInfo>(module =>
            {
                module.HasKey(x => x.Name);

                module.HasMany(x => x.Commands)
                    .WithOne(y => y.Module)
                    .HasForeignKey(z => z.ModuleName);
            });

            modelBuilder.Entity<CommandInfo>().HasKey(x => x.Name);
        }

        //Stolen from:
        //https://gitlab.com/QuantumToast/Administrator/blob/dev/Administrator/Database/AdminDatabaseContext.cs#L101-116
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

            if(expression is null)
                return await Guilds.FindAsync(guild.Id);

            return await Guilds.Include(expression).FirstOrDefaultAsync(x => x.Id == guild.Id);
        }

        public Task<IReadOnlyCollection<Guild>> GetAllGuildsAsync()
            => GetAllGuildsAsync<ulong>(null);

        public async Task<IReadOnlyCollection<Guild>> GetAllGuildsAsync<TProp>(Expression<Func<Guild, TProp>> expression)
        {
            if(expression is null)
            {
                return await Guilds.ToListAsync();
            }
            
            return await Guilds.Include(expression).ToListAsync();
        }

        public async Task<User> GetOrCreateUserAsync(IUser user)
        {
            var foundUser = await Users.FindAsync(user.Id)
                ?? await CreateUserAsync<ulong>(user, null);

            return foundUser;
        }

        public async Task<User> GetOrCreateUserAsync<TProp>(IUser user, Expression<Func<User, TProp>> expression)
        {
            var foundUser = await Users.Include(expression).FirstOrDefaultAsync(x => x.Id == user.Id)
                ?? await CreateUserAsync(user, expression);

            return foundUser;
        }

        private async Task<User> CreateUserAsync<TProp>(IUser user, Expression<Func<User, TProp>> expression)
        {
            var newUser = new User
            {
                Id = user.Id
            };

            await Users.AddAsync(newUser);

            await SaveChangesAsync();

            if(expression is null)
                return await Users.FindAsync(user.Id);

            return await Users.Include(expression).FirstOrDefaultAsync(x => x.Id == user.Id);
        }

        //async needed for the cast
        public async Task<IReadOnlyCollection<User>> GetAllUsersAsync()
            => await Users.ToListAsync();
        
    }
}
