using Discord;
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    /*
     * Message
     * Eval
     * shutdown
     * Sudo
     */

    [Name("Owner Commands")]
    [RequireOwner]
    public class Owner : EspeonBase
    {
        [Command("Message")]
        [Name("Message Channel")]
        public Task MessageChannelAsync(ulong channelId, [Remainder] string content)
        {
            var channel = Context.Client.GetChannel(channelId) as IMessageChannel;

            return channel is null ? SendNotOkAsync(0) : SendMessageAsync(content);
        }

        [Command("Eval")]
        [Name("Eval")]
        [RunMode(RunMode.Parallel)]
        public async Task EvalAsync([Remainder] string code)
        {
            var codes = Utilities.GetCodes(code);

            var assemblies = GetAssemblies();

            var usings = new[]
            {
                "Discord", "Discord.WebSocket",
                "Microsoft.Extensions.DependencyInjection",
                "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks",
                "Qmmands"
            };

            var namespaces = Assembly.GetEntryAssembly().GetTypes()
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
                Timestamp = DateTimeOffset.UtcNow,
                ThumbnailUrl = Context.Guild.CurrentUser.GetAvatarOrDefaultUrl()
            };

            var message = await SendMessageAsync(builder.Build());

            var sw = Stopwatch.StartNew();

            var toEval = codes.Count == 0 ? code : string.Join('\n', codes);

            var script = CSharpScript
                .Create($"{string.Join("", usings.Select(x => $"using {x};"))} {toEval}", 
                    scriptOptions, 
                    typeof(RoslynContext));

            var diagnostics = script.Compile();
            sw.Stop();

            var compilationTime = sw.ElapsedMilliseconds;

            if(diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error))
            {
                builder.WithDescription($"Compilation finished in: {compilationTime}ms");
                builder.WithColor(Color.Red);
                builder.WithTitle("Failed Evaluation");

                builder.AddField("Compilation Errors", string.Join('\n', diagnostics.Select(x => $"{x}")));

                await message.ModifyAsync(x => x.Embed = builder.Build());

                return;
            }

            var context = new RoslynContext
            {
                //base for clarity
                Context = base.Context,
                Services = base.Services
            };

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

                    switch (result.ReturnValue)
                    {
                        case string str:
                            builder.AddField($"{type}", $"\"{str}\"");
                            break;

                        case IEnumerable enumerable:

                            var list = enumerable.Cast<object>().ToList();

                            if(list.Count > 5)
                            {
                                builder.AddField($"{enumerable.GetType()}", "Enumerable has more than 5 elements");
                                break;
                            }

                            sb.AppendLine("```css");

                            foreach (var element in list)
                                sb.AppendLine($"[{element}]");

                            sb.AppendLine("```");

                            builder.AddField($"{enumerable.GetType()}", sb.ToString());

                            break;

                        default:

                            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                            if (props.Length == 0)
                            {
                                builder.AddField($"{type}", result.ReturnValue);
                                break;
                            }

                            var maxLength = props.Max(x => x.Name.Length);

                            foreach (var prop in props)
                            {
                                sb.AppendLine($"#{prop.Name.PadRight(maxLength, ' ')} - [{prop.GetValue(result.ReturnValue)}]");
                            }

                            var messages = Utilities.SplitByLength(sb.ToString(), 2000);

                            foreach (var msg in messages)
                                await SendMessageAsync($"```css\n{msg}\n```");

                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                sw.Stop();

                builder.WithDescription($"Code evaluated in {sw.ElapsedMilliseconds}ms but threw an exception");
                builder.WithColor(Color.Red);
                builder.WithTitle("Failed Evaluation");

                builder.AddField("Exception", ex);
            }

            await message.ModifyAsync(x => x.Embed = builder.Build());
        }

        [Command("shutdown")]
        [Name("Shutdown Bot")]
        public Task ShutdownAsync()
        {
            var cts = Services.GetService<CancellationTokenSource>();
            cts.Cancel(false);

            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<Assembly> GetAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location));

            foreach (var assembly in assemblies)
                yield return assembly;
        }

        [Command("sudo")]
        [Name("Sudo")]
        public Task SudoAsync([Remainder] string command)
        {
            return SendMessageAsync($"{Context.Guild.CurrentUser.Mention} {command}");
        }
    }
}
