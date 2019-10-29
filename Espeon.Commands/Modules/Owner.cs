using Disqord;
using Espeon.Core;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Module = Qmmands.Module;

namespace Espeon.Commands {
	/*
	* Message
	* Eval
	* shutdown
	* Sudo
	*/

	[Name("Owner Commands")]
	[RequireOwner]
	[Description("big boy commands")]
	public class Owner : EspeonModuleBase {
		[Command("Message")]
		[Name("Message Channel")]
		[Description("Sends a message to the specified channel")]
		public async Task MessageChannelAsync(ulong channelId, [Remainder] string content) {
			if (Client.GetChannel(channelId) is ITextChannel channel) {
				await channel.SendMessageAsync(content);
			} else {
				await SendNotOkAsync(0);
			}
		}

		[Command("Eval")]
		[Name("Eval")]
		[RunMode(RunMode.Parallel)]
		[Description("Evaluates C# code")]
		public async Task EvalAsync([Remainder] string code) {
			List<string> codes = Core.Utilities.GetCodes(code);

			IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies()
				.Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location));

			var usings = new[] {
				"Casino.Common",
				"Casino.Qmmands",
				"Casino.DependencyInjection",
				"Casino.Discord",
				"Discord",
				"Discord.WebSocket",
				"Microsoft.Extensions.DependencyInjection",
				"System",
				"System.Collections.Generic",
				"System.Linq",
				"System.Text",
				"System.Threading.Tasks",
				"Qmmands"
			};

			IEnumerable<string> namespaces = Assembly.GetEntryAssembly()?.GetTypes()
				.Where(x => !string.IsNullOrWhiteSpace(x.Namespace)).Select(x => x.Namespace).Distinct();

			ScriptOptions scriptOptions = ScriptOptions.Default
				.WithReferences(assemblies.Select(x => MetadataReference.CreateFromFile(x.Location)))
				.AddImports(namespaces);

			var builder = new LocalEmbedBuilder {
				Title = "Evaluating Code...",
				Color = Core.Utilities.EspeonColor,
				Description = "Waiting for completion...",
				Author = new LocalEmbedAuthorBuilder {
					IconUrl = Context.Member.GetAvatarUrl(),
					Name = Context.Member.DisplayName
				},
				Timestamp = DateTimeOffset.UtcNow
			};

			IUserMessage message = await SendMessageAsync(builder.Build());

			Stopwatch sw = Stopwatch.StartNew();

			string toEval = codes.Count == 0 ? code : string.Join('\n', codes);

			Script<object> script = CSharpScript.Create($"{string.Concat(usings.Select(x => $"using {x};"))} {toEval}",
				scriptOptions, typeof(RoslynContext));

			ImmutableArray<Diagnostic> diagnostics = script.Compile();
			sw.Stop();

			long compilationTime = sw.ElapsedMilliseconds;

			if (diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error)) {
				builder.WithDescription($"Compilation finished in: {compilationTime}ms");
				builder.WithColor(Color.Red);
				builder.WithTitle("Failed Evaluation");

				string GetDiagnosticsString() {
					return string.Join('\n', diagnostics.Select(x => $"{x}"));
				}

				builder.AddField("Compilation Errors", GetDiagnosticsString());

				await message.ModifyAsync(x => x.Embed = builder.Build());

				return;
			}

			var context = new RoslynContext(Context, Services);

			sw.Restart();

			try {
				ScriptState<object> result = await script.RunAsync(context);

				sw.Stop();
				builder.WithColor(Color.Green);

				builder.WithDescription($"Code compiled in {compilationTime}ms and ran in {sw.ElapsedMilliseconds}ms");
				builder.WithTitle("Code Evaluated");

				if (!(result.ReturnValue is null)) {
					var sb = new StringBuilder();
					Type type = result.ReturnValue.GetType();
					object rValue = result.ReturnValue;

					switch (rValue) {
						case Color col:
							builder.WithColor(col);
							builder.AddField("Colour", $"{col.RawValue}");
							break;

						case string str:
							builder.AddField($"{type}", $"\"{str}\"");
							break;

						case IEnumerable enumerable:

							List<object> list = enumerable.Cast<object>().ToList();
							Type enumType = enumerable.GetType();

							if (list.Count > 10) {
								builder.AddField($"{enumType}", "Enumerable has more than 10 elements");
								break;
							}

							if (list.Count > 0) {

								sb.AppendLine("```css");

								foreach (object element in list) {
									sb.Append('[').Append(element).AppendLine("]");
								}

								sb.AppendLine("```");
							} else {
								sb.AppendLine("Collection is empty");
							}

							builder.AddField($"{enumType}", sb.ToString());

							break;

						case Enum @enum:

							builder.AddField($"{@enum.GetType()}", $"```\n{@enum.Humanize()}\n```");

							break;

						default:

							List<string> messages = rValue.Inspect();

							if (type.IsValueType && messages.Count == 0) {
								builder.AddField($"{type}", rValue);
							}

							foreach (string msg in messages) {
								await SendMessageAsync($"```css\n{msg}\n```");
							}

							break;
					}
				}
			} catch (Exception ex) {
				sw.Stop();

				builder.WithDescription($"Code evaluated in {sw.ElapsedMilliseconds}ms but threw an exception");
				builder.WithColor(Color.Red);
				builder.WithTitle("Failed Evaluation");

				string str = ex.ToString();

				builder.AddField("Exception", Markdown.EscapeMarkdown(str.Length >= 600 ? str.Substring(0, 600) : str));
			} finally {
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			await message.ModifyAsync(x => x.Embed = builder.Build());
		}

		[Command("shutdown")]
		[Name("Shutdown Bot")]
		[Description("Shuts the bot down")]
		public Task ShutdownAsync() {
			var cts = Services.GetService<CancellationTokenSource>();
			cts.Cancel(false);

			return Task.CompletedTask;
		}

		[Command("sudo")]
		[Name("Sudo")]
		[Description("Runs a command as sudo")]
		public Task SudoAsync([Remainder] string command) {
			return SendMessageAsync($"{Context.Guild.CurrentMember.Mention} {command}");
		}

		[Command("reload")]
		[Name("Reload Responses")]
		[Description("Reloads the bots responses")]
		public Task ReloadResponsesAsync() {
			IReadOnlyList<Module> modules = Services.GetService<CommandService>().GetAllModules();
			Module[] filtered = modules.Where(x => !ulong.TryParse(x.Name, out _)).ToArray();

			Responses.LoadResponses(filtered);

			return SendOkAsync(0);
		}
	}
}