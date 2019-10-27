using Casino.Common;
using Casino.DependencyInjection;
using Casino.Qmmands;
using Discord;
using Discord.WebSocket;
using Espeon.Commands;
using Espeon.Core;
using Espeon.Core.Databases.CommandStore;
using Espeon.Core.Databases.GuildStore;
using Espeon.Core.Databases.UserStore;
using Espeon.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon {
	internal class Program {
		private static void Main() {
			using var cts = new CancellationTokenSource();
			new Program().MainAsync(cts).GetAwaiter().GetResult();
		}

		private async Task MainAsync(CancellationTokenSource cts) {
			Config config = Config.Create("./config.json");

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Type[] types = assemblies.SelectMany(x => x.GetTypes()
				.Where(y => typeof(BaseService<InitialiseArgs>).IsAssignableFrom(y) && !y.IsAbstract)).ToArray();

			var impls = new List<Type>();

			Type GetImpl(Type type) {
				Type[] interfaces = type.GetInterfaces();
				Type impl = Array.Find(interfaces, x => !typeof(IDisposable).IsAssignableFrom(x));

				impls.Add(impl);

				return impl;
			}

			Dictionary<Type, Type> dict = types.ToDictionary(GetImpl, x => x);

			IServiceProvider services = ConfigureServices(dict, config, cts);

			await using (var userStore = services.GetService<UserStore>()) //provides a scope for the variables
			{
				await using var guildStore = services.GetService<GuildStore>();
				await using var commandStore = services.GetService<CommandStore>();

				await userStore.Database.MigrateAsync();
				await guildStore.Database.MigrateAsync();
				await commandStore.Database.MigrateAsync();

				await services.RunInitialisersAsync(new InitialiseArgs {
					UserStore = userStore,
					GuildStore = guildStore,
					CommandStore = commandStore
				}, impls);

				await userStore.SaveChangesAsync();
				await guildStore.SaveChangesAsync();
				await commandStore.SaveChangesAsync();

				var espeon = new BotStartup(services, config);
				services.Inject(espeon);
				await espeon.StartAsync(userStore, commandStore);
			}

			await Task.Delay(-1, cts.Token);
		}

		private static IServiceProvider ConfigureServices(IDictionary<Type, Type> types,
			Config config, CancellationTokenSource cts) {
			return new ServiceCollection()
				.AddServices(types)
				.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig {
					ExclusiveBulkDelete = true,
					LogLevel = LogSeverity.Verbose,
					MessageCacheSize = 100
				})).AddSingleton(new CommandService(new CommandServiceConfiguration {
					StringComparison = StringComparison.InvariantCultureIgnoreCase,
					CooldownBucketKeyGenerator = (_, ctx) => {
						var context = (EspeonContext) ctx;
						return context.User.Id;
					}
				}).AddTypeParsers(typeof(EspeonContext).Assembly))
				.AddSingleton(config)
				.AddSingleton(cts)
				.AddSingleton(new TaskQueue(20))
				.AddSingleton<Random>()
				.AddConfiguredHttpClient()
				.AddEntityFrameworkNpgsql()
				.AddDbContext<UserStore>(ServiceLifetime.Transient)
				.AddDbContext<GuildStore>(ServiceLifetime.Transient)
				.AddDbContext<CommandStore>(ServiceLifetime.Transient)
				.BuildServiceProvider();
		}
	}
}