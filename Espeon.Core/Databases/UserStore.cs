using Discord;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon.Core.Databases.UserStore {
	public class UserStore : DbContext {
		private DbSet<User> Users { get; set; }
		public DbSet<Reminder> Reminders { get; set; }

		private static Config _config;

		public UserStore(Config config) {
			_config = config;
		}

		public UserStore() { }

#if !DEBUG
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(_config.ConnectionStrings.UserStore);
#else
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=UserStore;Username=postgres;Password=casino");
		}
#endif

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.Entity<User>(user => {
				user.HasKey(x => x.Id);

				user.Property(x => x.CandyAmount).HasDefaultValue(10);

				user.Property(x => x.HighestCandies).HasDefaultValue(10);

				user.Property(x => x.ResponsePack).HasDefaultValue(ResponsePack.Default)
					.HasConversion(y => (int) y, y => (ResponsePack) y);

				user.Property(x => x.ResponsePacks).HasDefaultValue(new List<ResponsePack> { ResponsePack.Default })
					.HasConversion(y => y.Select(x => (int) x).ToArray(),
						y => y.Select(x => (ResponsePack) x).ToList());

				user.HasMany(x => x.Reminders).WithOne();

				user.HasMany(x => x.Commands).WithOne();

				user.Property(x => x.BoughtEmotes).HasDefaultValue(false);
			});

			modelBuilder.Entity<Reminder>(reminder => {
				reminder.HasKey(x => x.Id);

				reminder.Property(x => x.WhenToRemove).HasConversion(y => y.ToUnixTimeMilliseconds(),
					y => DateTimeOffset.FromUnixTimeMilliseconds(y));

				reminder.Property(x => x.CreatedAt).HasConversion(y => y.ToUnixTimeMilliseconds(),
					y => DateTimeOffset.FromUnixTimeMilliseconds(y));
			});

			modelBuilder.Entity<DelayedCommand>(command => {
				command.HasKey(x => x.Id);

				command.Property(x => x.Id).ValueGeneratedOnAdd();

				command.Property(x => x.When).HasConversion(y => y.ToUnixTimeMilliseconds(),
					y => DateTimeOffset.FromUnixTimeMilliseconds(y));
			});
		}

		public async Task<User> GetOrCreateUserAsync(IUser user) {
			return await Users.FindAsync(user.Id) ?? await CreateUserAsync<ulong>(user, null);
		}

		public async Task<User> GetOrCreateUserAsync<TProp>(IUser user, Expression<Func<User, TProp>> expression) {
			return await Users.Include(expression).FirstOrDefaultAsync(x => x.Id == user.Id) ??
			       await CreateUserAsync(user, expression);
		}

		private async Task<User> CreateUserAsync<TProp>(IUser user, Expression<Func<User, TProp>> expression) {
			var newUser = new User { Id = user.Id };

			await Users.AddAsync(newUser);

			await SaveChangesAsync();

			if (expression is null) {
				return await Users.FindAsync(user.Id);
			}

			return await Users.Include(expression).FirstOrDefaultAsync(x => x.Id == user.Id);
		}

		//async needed for the cast
		public async Task<IReadOnlyCollection<User>> GetAllUsersAsync() {
			return await AsyncEnumerable.ToListAsync(Users, CancellationToken.None);
		}

		public async Task RemoveUserAsync(IUser user) {
			User dbUser = await GetOrCreateUserAsync(user);
			Users.Remove(dbUser);
		}
	}
}