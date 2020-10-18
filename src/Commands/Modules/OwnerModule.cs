using Disqord;
using Disqord.Bot;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Qmmands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon {
    [Name("Owner")]
    [Description("Owner only commands")]
    [BotOwnerOnly]
    public class OwnerModule : EspeonCommandModule{
        [Command("eval")]
        public async Task EvalAsync([Remainder] string rawCode) {
            var code = CommandHelper.GetCode(rawCode);
            var sw = Stopwatch.StartNew();
            var script = CSharpScript.Create(code, CommandHelper.RoslynScriptOptions, typeof(RoslynCommandContext));
            var diagnostics = script.Compile();
            var compilationTime = sw.ElapsedMilliseconds;
            
            if (diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)) {
                var builder = new LocalEmbedBuilder {
                    Title = "Compilation Failure",
                    Color = Color.Red,
                    Description = $"Compilation took {compilationTime}ms but failed due to..."
                };
                foreach (var diagnostic in diagnostics) {
                    var message = diagnostic.GetMessage();
                    builder.AddField(diagnostic.Id,
                        message.Substring(0, Math.Min(500, message.Length)));
                }

                await ReplyAsync(embed: builder.Build());
                return;
            }
            
            var context = new RoslynCommandContext(Context);
            var result = await script.RunAsync(context);
            sw.Stop();
            await ReplyAsync(result.ReturnValue.ToString());
        }
    }
}