using Disqord;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Espeon.Persistence {
    public class EspeonDbContext : DbContext {
        private DbSet<GuildPrefixes> GuildPrefixes { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
            => optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=Espeon;Username=postgres;Password=casino");

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<GuildPrefixes>(model => {
                model.HasIndex(prefixes => prefixes.GuildId).IsUnique();
                model.Property(prefixes => prefixes.GuildId).ValueGeneratedNever();
            });
        }

        public async Task<GuildPrefixes> GetPrefixesAsync(IGuild guild) {
            return await GuildPrefixes.SingleOrDefaultAsync(prefix => prefix.GuildId == guild.Id);
        }

        public async Task PersistGuildAsync(IGuild guild) {
            if (await GuildPrefixes.FindAsync(guild.Id.RawValue) is null) {
                await GuildPrefixes.AddAsync(new GuildPrefixes(guild.Id));
            }
            
            await SaveChangesAsync();
        }
        
        public async Task RemoveGuildAsync(IGuild guild) {
            var prefixes = await GuildPrefixes.FindAsync(guild.Id.RawValue);
            GuildPrefixes.Remove(prefixes);
            
            await SaveChangesAsync();
        }
        
        public async Task UpdateAsync<T>(T newData) where T : class {
            switch (newData) {
                case GuildPrefixes prefixes:
                    GuildPrefixes.Update(prefixes);
                    break;
            }
            
            await  SaveChangesAsync();
        }
    }
}