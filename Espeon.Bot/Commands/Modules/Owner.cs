using Casino.Discord;
using Discord;
using Espeon.Commands;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    /*
     * Message
     * Eval
     * shutdown
     * Sudo
     */

    [Name("Owner Commands")]
    [RequireOwner]
    [Description("big boy commands")]
    public class Owner : EspeonModuleBase
    {
        [Command("Message")]
        [Name("Message Channel")]
        [Description("Sends a message to the specified channel")]
        public Task MessageChannelAsync(ulong channelId, [Remainder] string content)
        {
            return !(Context.Client.GetChannel(channelId) is IMessageChannel channel)
                ? SendNotOkAsync(0)
                : channel.SendMessageAsync(content);
        }

        [Command("Eval")]
        [Name("Eval")]
        [RunMode(RunMode.Parallel)]
        [Description("Evaluates C# code")]
        public async Task EvalAsync([Remainder] string code)
        {
            var codes = Utilities.GetCodes(code);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location));

            var usings = new[]
            {
                "Casino.Common", "Casino.Qmmands", "Casino.DependencyInjection", "Casino.Discord",
                "Discord", "Discord.WebSocket",
                "Microsoft.Extensions.DependencyInjection",
                "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks",
                "Qmmands"
            };

            var namespaces = Assembly.GetEntryAssembly()?.GetTypes()
                .Where(x => !string.IsNullOrWhiteSpace(x.Namespace))
                .Select(x => x.Namespace)
                .Distinct();

            var scriptOptions = ScriptOptions.Default.WithReferences(assemblies
                .Select(x => MetadataReference.CreateFromFile(x.Location))).AddImports(namespaces);

            var builder = new EmbedBuilder
            {
                Title = "Evaluating Code...",
                Color = Utilities.EspeonColor,
                Description = "Waiting for completion...",
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = Context.User.GetDisplayName()
                },
                Timestamp = DateTimeOffset.UtcNow
            };

            var message = await SendMessageAsync(builder.Build());

            var sw = Stopwatch.StartNew();

            var toEval = codes.Count == 0 ? code : string.Join('\n', codes);

            var script = CSharpScript
                .Create($"{string.Concat(usings.Select(x => $"using {x};"))} {toEval}",
                    scriptOptions,
                    typeof(RoslynContext));

            var diagnostics = script.Compile();
            sw.Stop();

            var compilationTime = sw.ElapsedMilliseconds;

            if (diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error))
            {
                builder.WithDescription($"Compilation finished in: {compilationTime}ms");
                builder.WithColor(Color.Red);
                builder.WithTitle("Failed Evaluation");

                builder.AddField("Compilation Errors", string.Join('\n', diagnostics.Select(x => $"{x}")));

                await message.ModifyAsync(x => x.Embed = builder.Build());

                return;
            }

            var context = new RoslynContext(Context, Services);

            sw.Restart();

            try
            {
                var result = await script.RunAsync(context);

                sw.Stop();
                builder.WithColor(Color.Green);

                builder.WithDescription($"Code compiled in {compilationTime}ms and ran in {sw.ElapsedMilliseconds}ms");
                builder.WithTitle("Code Evaluated");

                if (!(result.ReturnValue is null))
                {
                    var sb = new StringBuilder();
                    var type = result.ReturnValue.GetType();
                    var rValue = result.ReturnValue;

                    switch (rValue)
                    {
                        case Color col:
                            builder.WithColor(col);
                            builder.AddField("Colour", $"{col.RawValue}");
                            break;

                        case string str:
                            builder.AddField($"{type}", $"\"{str}\"");
                            break;

                        case IEnumerable enumerable:

                            var list = enumerable.Cast<object>().ToList();
                            var enumType = enumerable.GetType();

                            if (list.Count > 10)
                            {
                                builder.AddField($"{enumType}", "Enumerable has more than 10 elements");
                                break;
                            }

                            if (list.Count > 0)
                            {

                                sb.AppendLine("```css");

                                foreach (var element in list)
                                    sb.Append('[').Append(element).AppendLine("]");

                                sb.AppendLine("```");
                            }
                            else
                            {
                                sb.AppendLine("Collection is empty");
                            }

                            builder.AddField($"{enumType}", sb.ToString());

                            break;

                        case Enum @enum:

                            builder.AddField($"{@enum.GetType()}", $"```\n{@enum.Humanize()}\n```");

                            break;

                        default:

                            var messages = rValue.Inspect();

                            if (type.IsValueType && messages.Count == 0)
                            {
                                builder.AddField($"{type}", rValue);
                            }

                            foreach (var msg in messages)
                                await SendMessageAsync($"```css\n{msg}\n```");

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                sw.Stop();

                builder.WithDescription($"Code evaluated in {sw.ElapsedMilliseconds}ms but threw an exception");
                builder.WithColor(Color.Red);
                builder.WithTitle("Failed Evaluation");

                var str = ex.ToString();

                builder.AddField("Exception", Format.Sanitize(str.Length >= 600 ? str.Substring(0, 600) : str));
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            await message.ModifyAsync(x => x.Embed = builder.Build());
        }

        [Command("shutdown")]
        [Name("Shutdown Bot")]
        [Description("Shuts the bot down")]
        public Task ShutdownAsync()
        {
            var cts = Services.GetService<CancellationTokenSource>();
            cts.Cancel(false);

            return Task.CompletedTask;
        }

        [Command("sudo")]
        [Name("Sudo")]
        [Description("Runs a command as sudo")]
        public Task SudoAsync([Remainder] string command)
        {
            return SendMessageAsync($"{Context.Guild.CurrentUser.Mention} {command}");
        }

        [Command("reload")]
        [Name("Reload Responses")]
        [Description("Reloads the bots responses")]
        public Task ReloadResponsesAsync()
        {
            var modules = Services.GetService<CommandService>().GetAllModules();
            var filtered = modules.Where(x => !ulong.TryParse(x.Name, out _)).ToArray();

            Responses.LoadResponses(filtered);

            return SendOkAsync(0);
        }
    }
}
