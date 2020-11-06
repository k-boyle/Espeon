using Disqord;
using Disqord.Bot.Parsers;
using Qmmands;
using System.Threading.Tasks;
using static Espeon.LocalisationStringKey;

namespace Espeon {
    public class IMemberTypeParser : EspeonTypeParser<IMember> {
        private readonly CachedMemberTypeParser _cachedParser;

        public IMemberTypeParser() {
            this._cachedParser = new CachedMemberTypeParser();
        }

        public override async ValueTask<TypeParserResult<IMember>> ParseAsync(
                Parameter parameter,
                string value,
                EspeonCommandContext context) {
            var cachedResult = await this._cachedParser.ParseAsync(parameter, value, context);
            
            if (cachedResult.IsSuccessful) {
                return TypeParserResult<IMember>.Successful(cachedResult.Value);
            }

            if (!Disqord.Discord.TryParseUserMention(value, out var id) && !Snowflake.TryParse(value, out id)) {
                return new EspeonTypeParserFailedResult<IMember>(MEMBER_NOT_IN_CACHE_NO_MENTION);
            }

            var member = await context.Guild.GetMemberAsync(id);
            return member is null
                ? new EspeonTypeParserFailedResult<IMember>(MEMBER_NOT_IN_GUILD)
                : TypeParserResult<IMember>.Successful(member);

        }
    }
}