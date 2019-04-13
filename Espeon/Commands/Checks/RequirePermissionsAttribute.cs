using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequirePermissionsAttribute : CheckAttribute
    {
        private readonly PermissionTarget _target;
        private readonly GuildPermission[] _guildPerms;
        private readonly ChannelPermission[] _channelPerms;

        public RequirePermissionsAttribute(PermissionTarget target, params GuildPermission[] guildPerms)
        {
            _target = target;
            _guildPerms = guildPerms;
            _channelPerms = new ChannelPermission[0];
        }

        public RequirePermissionsAttribute(PermissionTarget target, params ChannelPermission[] channelPerms)
        {
            _target = target;
            _channelPerms = channelPerms;
            _guildPerms = new GuildPermission[0];
        }

        public override ValueTask<CheckResult> CheckAsync(CommandContext originalContext, IServiceProvider provider)
        {
            var context = originalContext as EspeonContext;

            SocketGuildUser user = null;

            switch (_target)
            {
                case PermissionTarget.User:
                    user = context.User;
                    break;

                case PermissionTarget.Bot:
                    user = context.Guild.CurrentUser;
                    break;
            }

            var failedGuildPerms = _guildPerms.Where(guildPerm => !user.GuildPermissions.Has(guildPerm)).ToArray();

            var channelPerms = context.User.GetPermissions(context.Channel);

            var failedChannelPerms = _channelPerms.Where(channelPerm => !channelPerms.Has(channelPerm)).ToArray();

            if (failedGuildPerms.Length == 0 && failedChannelPerms.Length == 0)
                return new ValueTask<CheckResult>(CheckResult.Successful);

            var sb = new StringBuilder();

            foreach (var guildPerm in failedGuildPerms)
                sb.AppendLine($"{guildPerm}");

            foreach (var channelPerm in failedChannelPerms)
                sb.AppendLine($"{channelPerm}");

            return new ValueTask<CheckResult>(
                CheckResult.Unsuccessful(
                    $"{(_target == PermissionTarget.User ? "You" : "I")} need the following permissions to execute this command\n{sb}"));
        }
    }
}
