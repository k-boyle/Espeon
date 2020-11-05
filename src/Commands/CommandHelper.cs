using Disqord;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon {
    public static class CommandHelper {
        private const char BackTick = '`';
        private const char NewLine = '\n';
        
        public static ScriptOptions RoslynScriptOptions { get; }
        
        public static IReadOnlyDictionary<Type, string> ParameterExampleStrings { get; } = new Dictionary<Type, string> {
            [typeof(CachedMember)] = "<@376085382913064971>",
            [typeof(IMessage)] = "767451076084891678",
            [typeof(LocalCustomEmoji)] = "<:espeon:491227561385525248>",
            [typeof(CachedTextChannel)] = "#general",
            [typeof(CachedRole)] = "@Admins"
        };
        
        private static readonly string UsingsBlock;
        
        static CommandHelper() {
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
        
        public static async Task AddGlobalTagsAsync<T>(
                EspeonDbContext context,
                ICommandService commandService,
                ILogger<T> logger) {
            logger.LogInformation("Adding global tags");
            await context.GlobalTags.LoadAsync();
            commandService.AddModule<TagModule>(moduleBuilder => {
                foreach (var tag in context.GlobalTags) {
                    logger.LogDebug("Adding global tag {name}", tag.Key);
                    moduleBuilder.AddCommand(
                        context => GlobalTagCallbackAsync((EspeonCommandContext) context),
                        commandBuilder => commandBuilder.WithName(tag.Key).Aliases.Add(tag.Key));
                }

                logger.LogDebug("Created global tag module");
            });
        }
        
        private static async Task GlobalTagCallbackAsync(EspeonCommandContext context) {
            await using var dbContext = context.ServiceProvider.GetRequiredService<EspeonDbContext>();
            var tag = await dbContext.GlobalTags
                .FirstOrDefaultAsync(globalTag => globalTag.Key == context.Command.Name);
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