﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;
using Microsoft.Extensions.Options;
using Qmmands;
using static Espeon.LocalisationStringKey;

namespace Espeon {
    [Name("Misc")]
    [Description("Commands that doesn't really fit into a specific category")]
    public partial class MiscModule : EspeonCommandModule {
        public HttpClient Client { get; set; }
        public IOptions<Emotes> EmotesOptions { get; set; }

        [Name("Help")]
        [Description("Displays all the bots modules")]
        [Command("help")]
        public async Task HelpAsync() {
            var commandService = (ICommandService) Context.Bot;
            var modules = commandService.GetAllModules();
            
            var moduleStringJoiner = new StringJoiner(", ");
            
            foreach(var module in modules) {
                moduleStringJoiner.Append(Markdown.Code(module.Name));
            }

            var helpEmbedBuilder = new LocalEmbedBuilder {
                Color = Constants.EspeonColour,
                Title = "Espeon's Help",
                Author = new LocalEmbedAuthorBuilder {
                    IconUrl = Context.Member.GetAvatarUrl(),
                    Name = Context.Member.DisplayName
                },
                ThumbnailUrl = Context.Guild.CurrentMember.GetAvatarUrl(),
                Footer = new LocalEmbedFooterBuilder {
                    Text = $"Execute \"{GetPrefix()} module\" to view help for that module"
                },
                Fields = {
                    new LocalEmbedFieldBuilder {
                        Name = "Modules",
                        Value = moduleStringJoiner.ToString()
                    }
                }
            };

            var delete = new DeleteOnReaction(Context.User.Id, async () => await ReplyAsync(embed: helpEmbedBuilder.Build()));
            await Context.Channel.StartMenuAsync(delete);
        }
        
        [Name("Module Help")]
        [Description("View help for a specific module")]
        [Command("help")]
        public async Task HelpAsync([Remainder] Module module) {
            var (commandNamesString, commandAliasesString) = CreateCommandStrings(module.Commands);
            var submoduleString = CreateSubmoduleString(module.Submodules);

            var helpEmbedBuilder = CreateModuleHelpEmbed(
                module,
                commandNamesString,
                commandAliasesString,
                submoduleString);

            var delete = new DeleteOnReaction(Context.User.Id, async () => await ReplyAsync(embed: helpEmbedBuilder.Build()));
            await Context.Channel.StartMenuAsync(delete);
        }

        [Name("Command Help")]
        [Description("View help for specific commands")]
        [Command("help")]
        public async Task HelpAsync([Remainder] IEnumerable<Command> commands) {
            var embeds = commands.Select(CreateEmbedForCommandHelp).ToList();

            if (embeds.Count == 1) {
                var delete = new DeleteOnReaction(Context.User.Id, async () => await ReplyAsync(embed: embeds[0].Build()));
                await Context.Channel.StartMenuAsync(delete);
                return;
            }
            
            await SendPagedHelpAsync(embeds);
        }

        [Name("Mock")]
        [Description("Mocks a user")]
        [Command("mock", "m")]
        public async Task MockAsync(IMessage message) {
            var webhooks = await Context.Channel.GetWebhooksAsync();
            var webhook = webhooks.FirstOrDefault() ?? await Context.Channel.CreateWebhookAsync("Espeon Webhook");
            var webhookClient = new RestWebhookClient(webhook);

            static string Mock(string inStr) {
                return string.Create(inStr.Length, inStr, (span, state) => {
                    for (int i = span.Length - 1; i >= 0; i--) {
                        span[i] = (i & 1) == 0 ? state[i] : char.ToUpper(state[i]);
                    }
                });
            }

            if (string.IsNullOrWhiteSpace(message.Content)) {
                await ReplyAsync(MESSAGE_HAS_EMPTY_CONTENT);
                return;
            }
            
            await webhookClient.ExecuteAsync(
                Mock(message.Content),
                name: await GetDisplayNameAsync(message.Author),
                avatarUrl: message.Author.GetAvatarUrl());
        }
        
        
        [Name("Quote")]
        [Description("Quote a message")]
        [Command("quote", "q")]
        public async Task QuoteAsync(IMessage message) {
            var author = message.Author;
            var builder = new LocalEmbedBuilder {
                Color = Constants.EspeonColour,
                Description = message.Content,
                Author = new LocalEmbedAuthorBuilder {
                    IconUrl = author.GetAvatarUrl(),
                    Name = await GetDisplayNameAsync(author),
                    Url = GetJumpUrl(message)
                },
                Timestamp = message.CreatedAt,
                ImageUrl = GetImageUrl(message)
            };

            await ReplyAsync(embed: builder.Build());
        }

        private static string GetImageUrl(IMessage message) {
            static string GetAttachmentUrl(IUserMessage userMessage) {
                return userMessage.Attachments.FirstOrDefault() is { } attachment
                    ? attachment.Url
                    : string.Empty;
            }

            return message is IUserMessage userMessage
                ? GetAttachmentUrl(userMessage)
                : string.Empty;
        }

        private string GetJumpUrl(IMessage message) {
            var guild = Context.Bot.GetChannel(message.ChannelId) is CachedTextChannel channel
                ? channel.Guild
                : null;
            return message.GetJumpUrl(guild);
        }

        private async Task<string> GetDisplayNameAsync(IUser author) {
            var guildMember = await Context.Guild.GetOrFetchMemberAsync(author.Id);
            return guildMember?.DisplayName ?? author.Name;
        }

        [Name("Add Emote")]
        [Description("Adds the specified emote to the guild")]
        [Command("emote")]
        [RequireBotGuildPermissions(Permission.ManageEmojis)]
        [RequireMemberGuildPermissions(Permission.ManageEmojis)]
        public async Task StealEmoteAsync(LocalCustomEmoji emoji, [Example("pepowhatif")] string name = null) {
            await using var httpStream = await Client.GetStreamAsync(emoji.GetUrl());
            await using var memStream = new MemoryStream();
            await httpStream.CopyToAsync(memStream);
            memStream.Position = 0;
            var created = await Context.Guild.CreateEmojiAsync(memStream, name ?? emoji.Name);
            await ReplyAsync(created.ToString());
        }
        
        [Name("pepowhatif")]
        [Description("Sends a pepowhatif")]
        [Command("pepowhatif", "whatif", "pepo")]
        public async Task PepoWhatIfAsync(uint size = 0) {
            var emotes = EmotesOptions.Value;
            if (emotes.Strings.TryGetValue((EmoteKey) size, out var emoteStr)) {
                if (Context.Guild.CurrentMember.GetPermissionsFor(Context.Channel).ManageMessages) {
                    await Context.Message.DeleteAsync();
                }
                
                await ReplyAsync(emoteStr);
            } else {
                await ReplyAsync(INVALID_PEPOWHATIF_SIZE);
            }
        }
    }
}