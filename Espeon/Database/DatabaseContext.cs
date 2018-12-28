using Espeon.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Espeon.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<ModuleInfo> Modules { get; set; }
        public DbSet<CommandInfo> Commands { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql($"Host=127.0.0.1;Port=5432;Database=postgres;Username=postgres;Password=casino");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Guild>(guild =>
            {
                guild.HasKey(x => x.Id);

                guild.HasOne(x => x.Config)
                    .WithOne(y => y.Guild)
                    .HasForeignKey<Configuration>(z => z.GuildId);

                guild.HasOne(x => x.SpecialUsers)
                    .WithOne(y => y.Guild)
                    .HasForeignKey<ElevatedUsers>(z => z.GuildId);

                guild.HasOne(x => x.Data)
                    .WithOne(y => y.Guild)
                    .HasForeignKey<GuildData>(z => z.GuildId);

                guild.HasOne(x => x.Starboard)
                    .WithOne(y => y.Guild)
                    .HasForeignKey<Starboard>(z => z.GuildId);
            });

            modelBuilder.Entity<Configuration>(config =>
            {
                config.HasKey(x => x.GuildId);

                config.Property(x => x.RestrictedChannels)
                    .HasConversion(new SnowflakeCollectionParser());

                config.Property(x => x.RestrictedUsers)
                    .HasConversion(new SnowflakeCollectionParser());
            });

            modelBuilder.Entity<ElevatedUsers>(users =>
            {
                users.HasKey(x => x.GuildId);

                users.Property(x => x.Admins)
                    .HasConversion(new SnowflakeCollectionParser());

                users.Property(x => x.Moderators)
                    .HasConversion(new SnowflakeCollectionParser());
            });

            modelBuilder.Entity<GuildData>(data =>
            {
                data.HasKey(x => x.GuildId);

                data.HasMany(x => x.Commands)
                    .WithOne();

                data.Property(x => x.SelfAssigningRoles)
                    .HasConversion(new SnowflakeCollectionParser());
            });

            modelBuilder.Entity<Starboard>()
                .HasKey(x => x.GuildId);

            modelBuilder.Entity<CustomCommand>().HasKey(x => x.Id);

            modelBuilder.Entity<CustomCommand>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<User>(user =>
            {
                user.HasKey(x => x.Id);

                user.HasOne(x => x.Candies)
                    .WithOne(y => y.User)
                    .HasForeignKey<CandyData>(z => z.UserId);

                user.HasMany(x => x.Reminders)
                    .WithOne();
            });

            modelBuilder.Entity<CandyData>().HasKey(x => x.UserId);

            modelBuilder.Entity<Reminder>().HasKey(x => x.Id);

            modelBuilder.Entity<Reminder>().Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<ModuleInfo>(module =>
            {
                module.HasKey(x => x.Id);

                module.HasMany(x => x.Commands)
                    .WithOne();
            });

            modelBuilder.Entity<CommandInfo>().HasKey(x => x.Id);

            modelBuilder.Entity<CommandInfo>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
        }

        //Stolen from:
        //https://gitlab.com/QuantumToast/Administrator/blob/dev/Administrator/Database/AdminDatabaseContext.cs#L101-116
        public sealed class SnowflakeCollectionParser : ValueConverter<ICollection<ulong>, string>
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
    }
}
