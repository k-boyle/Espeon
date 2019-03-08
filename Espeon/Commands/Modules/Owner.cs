using Discord;
using Espeon.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MessageProperties = Espeon.Services.MessageService.MessageProperties;

namespace Espeon.Commands
{
    /*
     * Message
     * Eval
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

            IEnumerable<Assembly> GetAssemblies()
            {
                var entries = Assembly.GetEntryAssembly();
                foreach (var assembly in entries.GetReferencedAssemblies())
                    yield return Assembly.Load(assembly);
                yield return entries;
            }

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

            var scriptOptions = ScriptOptions.Default.WithReferences(GetAssemblies()
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

            var script = CSharpScript
                .Create($"{string.Join("", usings.Select(x => $"using {x};"))} {string.Join('\n', codes)}", 
                    scriptOptions, 
                    typeof(EvalContext));

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

            var context = new EvalContext
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
                    builder.AddField("Returned Type", result.ReturnValue.GetType());
                    builder.AddField("Returned Value", result.ReturnValue);
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

        public class EvalContext
        {
            public EspeonContext Context { get; set; }
            public IServiceProvider Services { get; set; }

            public Task<IUserMessage> SendMessageAsync(Action<MessageProperties> func)
            {
                var message = Services.GetService<MessageService>();

                return message.SendMessageAsync(Context, func);
            }
        }
    }
}
