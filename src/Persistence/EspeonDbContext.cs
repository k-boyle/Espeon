using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Disqord.Bot.Prefixes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Espeon {
    public class EspeonDbContext : DbContext {
        private const string MentionPrefixLiteral = "<mention>";

        private readonly Dictionary<Type, IEnumerable> _dbSets;

        public DbSet<GuildPrefixes> GuildPrefixes { get; set; }
        public DbSet<UserLocalisation> UserLocalisations { get; set; }
        public DbSet<UserReminder> UserReminders { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<GuildTags> GuildTags { get; set; }
        public DbSet<GlobalTag> GlobalTags { get; set; }

        public EspeonDbContext(DbContextOptions options) : base(options) {
            this._dbSets = new Dictionary<Type, IEnumerable> {
                [typeof(GuildPrefixes)] = GuildPrefixes,
                [typeof(UserLocalisation)] = UserLocalisations,
                [typeof(UserReminder)] = UserReminders,
                [typeof(Tag)] = Tags,
                [typeof(GuildTags)] = GuildTags,
                [typeof(GlobalTag)] = GlobalTags,
            };
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
                        .HasConversion(new EnumToNumberConverter<Language, int>());
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
        
        public async Task<TEntity> GetOrCreateAsync<TEntity, TKey>(TKey key, Func<TKey, TEntity> newEntitySupplier)
                where TEntity : class {
            var dbSet = GetDbSet<TEntity>();
            return await this.GetOrCreateAsync(dbSet, key, newEntitySupplier);
        }
        
        public async Task<TEntity> GetOrCreateAsync<TEntity, TKey>(TKey key1, TKey key2, Func<TKey, TKey, TEntity> newEntitySupplier)
                where TEntity : class {
            return await this.GetOrCreateAsync(GetDbSet<TEntity>(), key1, key2, newEntitySupplier);
        }
        
        public async Task<TEntity> IncludeAndFindAsync<TEntity, TProperty, TKey>(
                TKey key,
                Expression<Func<TEntity, IEnumerable<TProperty>>> navigationExpression)
                    where TEntity : class where TProperty : class {
            return await this.IncludeAndFindAsync(GetDbSet<TEntity>(), key, navigationExpression);
        }
        
        public async Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class {
            await this.UpdateAsync(GetDbSet<TEntity>(), entity);
        }
        
        public async Task PersistAsync<TEntity>(TEntity entity) where TEntity : class {
            await this.PersistAsync(GetDbSet<TEntity>(), entity);
        }
        
        public async Task RemoveAsync<TEntity>(TEntity entity) where TEntity : class {
            await this.RemoveAsync(GetDbSet<TEntity>(), entity);
        }

        private DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class {
            return (DbSet<TEntity>) this._dbSets[typeof(TEntity)];
        }
    }
}