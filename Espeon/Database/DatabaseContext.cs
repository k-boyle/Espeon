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
    }
}
