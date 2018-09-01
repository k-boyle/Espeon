using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Umbreon.Extensions;
using Umbreon.Interactive;
using Umbreon.Interactive.Callbacks;
using Umbreon.Interactive.Criteria;
using Umbreon.Interactive.Paginator;
using Umbreon.Services;

namespace Umbreon.Paginators.CommandMenu
{
    public class CommandMenuCallback : IReactionCallback, ICallback
    {
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly InteractiveService _interactive;
        private readonly CommandMenuMessage _properties;
        private readonly MessageService _message;

        private string _currentMenu;
        private bool _executing;
        private bool _isMain = true;
        private int _selectedIndex;

        public IUserMessage Message { get; private set; }
        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion => new EnsureReactionFromSourceUserCriterion();
        public TimeSpan? Timeout => TimeSpan.FromMinutes(2);
        public ICommandContext Context { get; }
        
        public CommandMenuCallback(CommandService commands, IServiceProvider services, InteractiveService interactive, CommandMenuMessage properties, MessageService message, ICommandContext context)
        {
            _commands = commands;
            _services = services;
            _interactive = interactive;
            _properties = properties;
            _message = message;
            Context = context;
        }

        public async Task DisplayAsync()
        {
            var message = await Context.Channel.SendMessageAsync(string.Empty, embed: BuildEmbed());
            Message = message;
            _interactive.AddReactionCallback(message, this);
            _ = Task.Run(async () => { await message.AddReactionsAsync(_properties.Emojis.Values, new RequestOptions
            {
                BypassBuckets = true
            }); });
            if (Timeout.HasValue)
                _ = Task.Delay(Timeout.Value).ContinueWith(async _ =>
                {
                    _interactive.RemoveReactionCallback(message);
                    await Message.DeleteAsync();
                });
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;
            var emotes = _properties.Emojis;
            var count = _isMain
                ? _properties.CommandsDictionary.Keys.Count
                : _properties.CommandsDictionary.FirstOrDefault(x =>
                    string.Equals(x.Key.Name, _currentMenu, StringComparison.CurrentCultureIgnoreCase)).Value.Count();

            if (_executing) return false;

            if (emote.Equals(emotes["up"]))
            {
                if (_selectedIndex == 0)
                    _selectedIndex = count - 1;
                else
                    _selectedIndex--;
            }
            else if (emote.Equals(emotes["down"]))
            {
                if (_selectedIndex == count - 1)
                    _selectedIndex = 0;
                else
                    _selectedIndex++;
            }
            else if (emote.Equals(emotes["back"]))
            {
                _isMain = true;
                _selectedIndex = 0;
            }
            else if (emote.Equals(emotes["select"]))
            {
                if (_isMain)
                {
                    _isMain = false;
                    _currentMenu = _properties.CommandsDictionary.Keys.ElementAt(_selectedIndex).Name;
                    _selectedIndex = 0;
                }
                else
                {
                    _executing = true;
                    var selectedCommand = _properties.CommandsDictionary.FirstOrDefault(x =>
                            string.Equals(x.Key.Name, _currentMenu, StringComparison.CurrentCultureIgnoreCase)).Value
                        .ElementAt(_selectedIndex);
                    _ = Task.Run(async () =>
                    {
                        var paramValues = new StringBuilder();
                        var execute = true;
                        foreach (var param in selectedCommand.Parameters)
                        {
                            var criteria = new Criteria<SocketMessage>()
                                .AddCriterion(new EnsureSourceChannelCriterion())
                                .AddCriterion(new EnsureFromUserCriterion(reaction.UserId));
                            await _message.SendMessageAsync(Context,
                                $"What do you want the {param.Name} to be? Respond with `cancel` to cancel execution");
                            var response =
                                await _interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(15));
                            if (response is null ||
                                response.Content.Equals("cancel", StringComparison.CurrentCultureIgnoreCase))
                            {
                                execute = false;
                                break;
                            }

                            paramValues.AppendJoin(' ', response.Content);
                        }

                        if (execute)
                        {
                            var result = await _commands.ExecuteAsync(Context,
                                $"{selectedCommand.Aliases.FirstOrDefault()} {paramValues}", _services);
                            if (!result.IsSuccess)
                                await Context.Channel.SendMessageAsync(result.ErrorReason);
                        }

                        _executing = false;
                    });
                }
            }
            else if (emote.Equals(emotes["delete"]))
            {
                await Message.DeleteAsync();
                return true;
            }

            _ = Message.RemoveReactionAsync(emote, reaction.User.Value);
            await Message.ModifyAsync(x => x.Embed = BuildEmbed());

            return false;
        }

        private Embed BuildEmbed()
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = (Context.User as SocketGuildUser).GetDisplayName()
                },
                Color = Color.DarkTeal,
                Title = "Umbreon's Command Menu",
                Description = "A menu to help you navigate and use commands",
                Timestamp = DateTimeOffset.UtcNow,
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarOrDefaultUrl()
            };
            embed.AddEmptyField();
            var i = 0;
            var builder = new StringBuilder();
            if (_isMain)
            {
                foreach (var module in _properties.CommandsDictionary.Keys)
                    builder.AppendLine(i++ == _selectedIndex
                        ? $"**>{(ulong.TryParse(module.Name, out _) ? Context.Guild.Name : module.Name)}**"
                        : ulong.TryParse(module.Name, out _)
                            ? Context.Guild.Name
                            : module.Name);

                embed.AddField("Modules", builder.ToString());
            }
            else
            {
                var commands = _properties.CommandsDictionary.FirstOrDefault(x =>
                    string.Equals(x.Key.Name, _currentMenu, StringComparison.CurrentCultureIgnoreCase)).Value;
                foreach (var command in commands)
                    builder.AppendLine(i++ == _selectedIndex ? $"**>{command.Name}**" : command.Name);

                embed.AddField($"{(ulong.TryParse(_currentMenu, out _) ? Context.Guild.Name : _currentMenu)} Commands",
                    builder.ToString());
            }

            return embed.Build();
        }
    }
}