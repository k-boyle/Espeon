using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Prefixes;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Qmmands;
using static Espeon.CommandHelper;

namespace Espeon {
    public partial class MiscModule {
        private static string CreateSubmoduleString(IEnumerable<Module> executableSubmodules) {
            var subModuleStringJoiner = new StringJoiner(", ");
            foreach (var submodule in executableSubmodules) {
                subModuleStringJoiner.Append(Markdown.Code(submodule.Name));
            }

            var submoduleString = subModuleStringJoiner.ToString();
            return submoduleString;
        }

        private static (string, string) CreateCommandStrings(IEnumerable<Command> executableCommands) {
            var commandNameStringJoined = new StringJoiner(", ");
            var commandAliasStringJoiner = new StringJoiner(", ");
            foreach (var command in executableCommands) {
                commandNameStringJoined.Append(Markdown.Code(command.Name));

                if (command.Aliases.Count > 0) {
                    commandAliasStringJoiner.Append(Markdown.Code(command.Aliases.First()));
                }
            }

            return (commandNameStringJoined.ToString(), commandAliasStringJoiner.ToString());
        }

        private LocalEmbedBuilder CreateModuleHelpEmbed(
                Module module,
                string commandNamesString,
                string commandAliasesString,
                string submoduleString) {
            var helpEmbedBuilder = new LocalEmbedBuilder {
                Color = Constants.EspeonColour,
                Title = $"{module.Name} Help",
                Author = new LocalEmbedAuthorBuilder {
                    IconUrl = Context.Member.GetAvatarUrl(),
                    Name = Context.Member.DisplayName
                },
                ThumbnailUrl = Context.Guild.CurrentMember.GetAvatarUrl(),
                Description = module.Description,
                Footer = new LocalEmbedFooterBuilder {
                    Text = $"Execute \"{GetPrefix()} command\" to view help for that specific command"
                }
            };

            if (module.Parent != null) {
                helpEmbedBuilder.AddField("Parent Module", Markdown.Code(module.Parent.Name));
            }

            if (module.FullAliases.Count > 0) {
                helpEmbedBuilder.AddField("Module Aliases", string.Join(", ", module.FullAliases.Select(Markdown.Code)));
            }
            
            helpEmbedBuilder.AddField("Command Names", commandNamesString);
            helpEmbedBuilder.AddField("Command Aliases", commandAliasesString);

            if (submoduleString.Length > 0) {
                helpEmbedBuilder.AddField("Submodules", submoduleString);
            }

            return helpEmbedBuilder;
        }
        
        private LocalEmbedBuilder CreateEmbedForCommandHelp(Command command) {
            string GetCommandSignature() {
                var commandSignatureJoiner = new StringJoiner(" ");
                commandSignatureJoiner.Append(command.FullAliases.First());

                foreach (var parameter in command.Parameters) {
                    var paramString = GetParameterString(parameter);
                    commandSignatureJoiner.Append(Markdown.Code(paramString));
                }

                return commandSignatureJoiner.ToString();
            }

            var helpEmbedBuilder = new LocalEmbedBuilder {
                Color = Constants.EspeonColour,
                Title = $"{command.Name} Help",
                Author = new LocalEmbedAuthorBuilder {
                    IconUrl = Context.Member.GetAvatarUrl(),
                    Name = Context.Member.DisplayName
                },
                ThumbnailUrl = Context.Guild.CurrentMember.GetAvatarUrl(),
                Description = command.Description,
                Footer = new LocalEmbedFooterBuilder {
                    Text = $"You can't go deeper 👀, \"{GetPrefix()} command\" to execute a command"
                },
                Fields = {
                    new LocalEmbedFieldBuilder {
                        Name = "Module",
                        Value = Markdown.Code(command.Module.Name)
                    },
                    new LocalEmbedFieldBuilder {
                        Name = "Command Signature",
                        Value = GetCommandSignature()
                    }
                }
            };

            AddCommandAliases(command, helpEmbedBuilder);
            AddCommandFullAliases(command, helpEmbedBuilder);
            AddParameterExamples(command, helpEmbedBuilder);

            return helpEmbedBuilder;
        }

        private static string GetParameterString(Parameter parameter) {
            var paramString = parameter.Name;

            if (parameter.IsRemainder) {
                paramString = $"{paramString}...";
            }

            if (parameter.IsOptional) {
                paramString = $"{paramString} = {parameter.DefaultValue ?? "\"\""}";
            }

            paramString = $"[{paramString}]";
            return paramString;
        }

        private static void AddParameterExamples(Command command, LocalEmbedBuilder helpEmbedBuilder) {
            if (command.Parameters.Count <= 0) {
                return;
            }

            static string GetExampleHelpString(Parameter parameter) {
                string GetExampleStringFromHelper() {
                    return ParameterExampleStrings.TryGetValue(parameter.Type, out var str)
                        ? str
                        : "EXAMPLE_MISSING";
                }

                var exampleAttribute = parameter.Attributes.OfType<ExampleAttribute>().FirstOrDefault();
                return $"**{parameter.Name}**: {exampleAttribute?.Value ?? GetExampleStringFromHelper()}";
            }

            var parameterHelp = string.Join('\n', command.Parameters.Select(GetExampleHelpString));

            helpEmbedBuilder.AddField("Parameter Examples", parameterHelp);
        }

        private static void AddCommandFullAliases(Command command, LocalEmbedBuilder helpEmbedBuilder) {
            if (command.Module.FullAliases.Count > 0) {
                helpEmbedBuilder.Fields.Insert(
                    2,
                    new LocalEmbedFieldBuilder {
                        Name = "Full Aliases",
                        Value = string.Join(", ", command.FullAliases.Select(Markdown.Code))
                    });
            }
        }

        private static void AddCommandAliases(Command command, LocalEmbedBuilder helpEmbedBuilder) {
            if (command.Aliases.Count <= 0) {
                return;
            }
            
            var aliases = command.Aliases.Where(alias => !string.IsNullOrWhiteSpace(alias)).Select(Markdown.Code);
            helpEmbedBuilder.Fields.Insert(
                1,
                new LocalEmbedFieldBuilder {
                    Name = "Aliases",
                    Value = string.Join(", ", aliases)
                });
        }

        private async Task SendPagedHelpAsync(IReadOnlyList<LocalEmbedBuilder> embeds) {
            var pages = new List<Page>();
            for (var i = 0; i < embeds.Count; i++) {
                var embed = embeds[i];
                embed.Footer = new LocalEmbedFooterBuilder {
                    Text = $"{embed.Footer.Text} [{i + 1}/{embeds.Count}]"
                };

                pages.Add(embed.Build());
            }

            var pagedProvider = new DefaultPageProvider(pages);
            var menu = new PagedMenu(Context.Member.Id, pagedProvider);
            await Context.Channel.StartMenuAsync(menu);
        }

        private string GetPrefix() {
            return Context.Prefix is MentionPrefix 
                ? $"@{Context.Guild.CurrentMember.Name}"
                : Context.Prefix.ToString();
        }
    }
}