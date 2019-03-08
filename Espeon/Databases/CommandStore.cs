using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Espeon.Databases.CommandStore
{
    public class CommandStore : DbContext
    {
        public DbSet<ModuleInfo> Modules { get; set; }

        private static Config _config;

        public CommandStore(Config config)
            => _config = config;

        public CommandStore()
        {
        }

#if !DEBUG
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(_config.ConnectionStrings.CommandStore);
#else
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=CommandStore;Username=postgres;Password=casino");
#endif

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModuleInfo>(module =>
            {
                module.HasKey(x => x.Name);

                module.HasMany(x => x.Commands)
                    .WithOne(y => y.Module)
                    .HasForeignKey(z => z.ModuleName);
            });

            modelBuilder.Entity<CommandInfo>(command =>
            {
                command.HasKey(x => new { x.Name, x.ModuleName });

                command.Property(x => x.Responses)
                    .HasConversion(
                    y => JsonConvert.SerializeObject(y, Formatting.Indented),
                    y => JsonConvert.DeserializeObject<Dictionary<ResponsePack, string[]>>(y));
            });
        }
        
        /* leaving this here for the lolz
        private class DictionaryParser : ValueConverter<IDictionary<ResponsePack, string[]>, string>
        {
            public DictionaryParser(ConverterMappingHints mappingHints = null)
                : base(InExpression, OutExpression, mappingHints)
            {
            }

            private static readonly Expression<Func<IDictionary<ResponsePack, string[]>, string>> InExpression
                = collection => collection != null && collection.Any()
                ? string.Join(";;", collection.Select(x => $"{(int)x.Key}{string.Join("::", x.Value)}"))
                : null;

            private static readonly Expression<Func<string, IDictionary<ResponsePack, string[]>>> OutExpression
                = input => !string.IsNullOrWhiteSpace(input)
                ? input.Split(";;", StringSplitOptions.RemoveEmptyEntries)
                    .ToDictionary(
                    x => (ResponsePack)int.Parse($"{x[0]}"), 
                    x => x.Substring(1).Split("::", StringSplitOptions.RemoveEmptyEntries))
                : new Dictionary<ResponsePack, string[]>();
        }
        */
    }
}
