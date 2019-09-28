using Discord;
using Discord.WebSocket;
using Espeon.Commands;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    public class RequirePermissionsAttribute : EspeonCheckBase
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

        public override ValueTask<CheckResult> CheckAsync(EspeonContext context, IServiceProvider provider)
        {
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

            var channelPerms = user.GetPermissions(context.Channel);

            var failedChannelPerms = _channelPerms.Where(channelPerm => !channelPerms.Has(channelPerm)).ToArray();

            if (failedGuildPerms.Length == 0 && failedChannelPerms.Length == 0)
                return CheckResult.Successful;

            var sb = new StringBuilder();

            foreach (var guildPerm in failedGuildPerms)
                sb.AppendLine(guildPerm);

            foreach (var channelPerm in failedChannelPerms)
                sb.AppendLine(channelPerm);

            var u = context.Invoker;
            var response = provider.GetService<IResponseService>();

            var target = _target == PermissionTarget.User ? "You" : "I";

            return CheckResult.Unsuccessful(
                response.GetResponse(this, u.ResponsePack, 0, target, sb));
        }
    }
}
