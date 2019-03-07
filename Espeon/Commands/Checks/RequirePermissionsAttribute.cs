using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PermissionTarget = Espeon.Enums.PermissionTarget;

namespace Espeon.Commands.Checks
{
    public class RequirePermissionsAttribute : CheckBaseAttribute
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

        public override Task<CheckResult> CheckAsync(ICommandContext originalContext, IServiceProvider provider)
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

            var failedGuildPerms = new List<GuildPermission>();

            foreach (var guildPerm in _guildPerms)
            {
                if (!user.GuildPermissions.Has(guildPerm))
                    failedGuildPerms.Add(guildPerm);
            }

            var failedChannelPerms = new List<ChannelPermission>();
            var channelPerms = context.User.GetPermissions(context.Channel);

            foreach (var channelPerm in _channelPerms)
            {
                if (!channelPerms.Has(channelPerm))
                    failedChannelPerms.Add(channelPerm);
            }

            if (failedGuildPerms.Count == 0 && failedChannelPerms.Count == 0)
                return Task.FromResult(CheckResult.Successful);

            var sb = new StringBuilder();

            foreach (var guildPerm in failedGuildPerms)
                sb.AppendLine($"{guildPerm}");

            foreach (var channelPerm in failedChannelPerms)
                sb.AppendLine($"{channelPerm}");

            return Task.FromResult(
                CheckResult.Unsuccessful(
                    $"{(_target == PermissionTarget.User ? "You" : "I")} need the following permissions to execute this command\n{sb}"));
        }
    }
}
