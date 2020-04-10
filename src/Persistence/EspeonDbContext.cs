﻿using Disqord;
using Disqord.Bot.Prefixes;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Persistence {
    public class EspeonDbContext : DbContext {
        private const string MentionPrefixLiteral = "<mention>";
        
        private DbSet<GuildPrefixes> GuildPrefixes { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
            => optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=Espeon;Username=postgres;Password=casino");

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<GuildPrefixes>(model => {
                model.HasIndex(prefixes => prefixes.GuildId).IsUnique();
                model.Property(prefixes => prefixes.GuildId).ValueGeneratedNever();
                model.Property(prefixes => prefixes.Values).HasConversion(
                    prefixes => prefixes.Select(x => x.ToString()).ToArray(),
                    arr => new HashSet<IPrefix>(arr.Select(ParseStringAsPrefix)));
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
            
            await SaveChangesAsync();
        }
        
        private IPrefix ParseStringAsPrefix(string value) {
            return string.Equals(value, MentionPrefixLiteral) 
                ? MentionPrefix.Instance as IPrefix
                : new StringPrefix(value);
        }
    }
}