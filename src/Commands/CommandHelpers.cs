using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon {
    public static class CommandHelpers {
        private const char BackTick = '`';
        private const char NewLine = '\n';
        
        public static ScriptOptions RoslynScriptOptions { get; }
        private static readonly string UsingsBlock;
        
        static CommandHelpers() {
            var rawUsings = new[] {
                "Disqord.Bot",
                "Microsoft.Extensions.DependencyInjection",
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "System.Text",
                "System.Threading.Tasks",
                "Qmmands"
            };
            UsingsBlock = string.Concat(rawUsings.Select(str => $"using {str}; "));
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location));
            
            var namespaces = Assembly.GetEntryAssembly()?.GetTypes()
                .Where(x => !string.IsNullOrWhiteSpace(x.Namespace)).Select(x => x.Namespace).Distinct();

            RoslynScriptOptions = ScriptOptions.Default
                .WithReferences(assemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location)))
                .AddImports(namespaces);
        }
        
        public static async Task GlobalTagCallbackAsync(EspeonCommandContext context) {
            await using var dbContext = context.ServiceProvider.GetRequiredService<EspeonDbContext>();
            var tag = await dbContext.GetTagAsync<GlobalTag>(context.Command.Name);
            await context.Channel.SendMessageAsync(tag.Value);
            tag.Uses++;
            await dbContext.UpdateAsync(tag);
        }

        public static string GetCode(string rawCode) {
            static string GetCode(string inCode) {
                if (inCode[0] != BackTick) {
                    return inCode;
                }
                
                if (inCode[1] != BackTick) {
                    return inCode.Substring(1, inCode.Length - 2);
                }
                
                var startIndex = inCode.IndexOf(NewLine);
                if (startIndex == -1) {
                    throw new ArgumentException("Format your code blocks properly >:[");
                }

                return inCode.Substring(startIndex + 1, inCode.Length - startIndex - 5);
            }

            var code = GetCode(rawCode);
            return string.Concat(UsingsBlock, code);
        }
    }
}