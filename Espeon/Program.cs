﻿using Disqord;
using Espeon.Commands;
using Espeon.Core;
using Espeon.Core.Database.CommandStore;
using Espeon.Core.Database.GuildStore;
using Espeon.Core.Database.UserStore;
using Espeon.Services;
using Kommon.Common;
using Kommon.DependencyInjection;
using Kommon.Qmmands;
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
			MainAsync(cts).GetAwaiter().GetResult();
		}

		private static async Task MainAsync(CancellationTokenSource cts) {
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

			using (var userStore = services.GetService<UserStore>()) //provides a scope for the variables
			{
				using var guildStore = services.GetService<GuildStore>();
				using var commandStore = services.GetService<CommandStore>();

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
				.AddSingleton(new DiscordClient(TokenType.Bot, config.DiscordToken, new DiscordClientConfiguration() {
						MessageCache = new DefaultMessageCache(20),
						// GuildSubscriptions = false
				}))
				.AddSingleton(new CommandService(new CommandServiceConfiguration {
					StringComparison = StringComparison.InvariantCultureIgnoreCase,
					CooldownBucketKeyGenerator = (_, ctx) => {
						var context = (EspeonContext) ctx;
						return context.Member.Id;
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