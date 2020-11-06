using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Espeon {
    // todo common
    public static class EFCoreExtensions {
        public static async Task<TEntity> GetOrCreateAsync<TEntity, TKey>(
                this DbContext context,
                DbSet<TEntity> dbSet,
                TKey key,
                Func<TKey, TEntity> newEntitySupplier)
                    where TEntity : class {
            var entity = await dbSet.FindAsync(key);
            
            if (entity != null) {
                return entity;
            }

            entity = newEntitySupplier(key);
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();

            return entity;
        }
        
        public static async Task<TEntity> GetOrCreateAsync<TEntity, TKey>(
                this DbContext context,
                DbSet<TEntity> dbSet,
                TKey key1,
                TKey key2,
                Func<TKey, TKey, TEntity> newEntitySupplier)
                    where TEntity : class {
            var entity = await dbSet.FindAsync(key1, key2);
            
            if (entity != null) {
                return entity;
            }

            entity = newEntitySupplier(key1, key2);
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();

            return entity;
        }

        public static async Task<TEntity> IncludeAndFindAsync<TEntity, TProperty, TKey>(
                this DbContext context,
                DbSet<TEntity> dbSet,
                TKey key,
                Expression<Func<TEntity, IEnumerable<TProperty>>> navigationExpression)
                    where TEntity : class where TProperty : class {
            var entity = await dbSet.FindAsync(key);

            if (entity != null) {
                await context.Entry(entity).Collection(navigationExpression).LoadAsync();
            }

            return entity;
        }

        public static async Task UpdateAsync<TEntity>(this DbContext context, DbSet<TEntity> dbSet, TEntity entity)
                where TEntity : class {
            dbSet.Update(entity);
            await context.SaveChangesAsync();
        }
        
        public static async Task PersistAsync<TEntity>(this DbContext context, DbSet<TEntity> dbSet, TEntity entity)
                where TEntity : class {
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();
        }
        
        public static async Task RemoveAsync<TEntity>(this DbContext context, DbSet<TEntity> dbSet, TEntity entity)
                where TEntity : class {
            dbSet.Remove(entity);
            await context.SaveChangesAsync();
        }
        
        // should only be used if FirstOrDefaultAsync can't be translated into an expression
        public static async Task<TEntity> FirstOrDefault2Async<TEntity>(this DbSet<TEntity> dbSet, Predicate<TEntity> predicate)
                where TEntity : class {
            await foreach(var entity in dbSet) {
                if (predicate(entity)) {
                    return entity;
                }
            }

            return null;
        }
    }
}