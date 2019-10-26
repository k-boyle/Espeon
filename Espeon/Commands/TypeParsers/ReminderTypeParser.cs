using Espeon.Core;
using Espeon.Core.Commands;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class ReminderTypeParser : EspeonTypeParser<(string, TimeSpan)> {
		private static readonly RequireSpecificLengthAttribute SpecificLengthAttribute;
		private static readonly TimeSpanTypeParser TimeSpanTypeParser;

		static ReminderTypeParser() {
			SpecificLengthAttribute = new RequireSpecificLengthAttribute(0, 200);
			TimeSpanTypeParser = new TimeSpanTypeParser();
		}

		public override async ValueTask<TypeParserResult<(string, TimeSpan)>> ParseAsync(Parameter parameter,
			string value, EspeonContext context, IServiceProvider provider) {
			CheckResult lengthResult = await SpecificLengthAttribute.CheckAsync(value, context, provider);

			if (!lengthResult.IsSuccessful) {
				return TypeParserResult<(string, TimeSpan)>.Unsuccessful(lengthResult.Reason);
			}

			TypeParserResult<TimeSpan> timeParserResult =
				await TimeSpanTypeParser.ParseAsync(parameter, value, context, provider);

			if (!timeParserResult.IsSuccessful) {
				return TypeParserResult<(string, TimeSpan)>.Unsuccessful(timeParserResult.Reason);
			}

			return TypeParserResult<(string, TimeSpan)>.Successful((Utilities.TimeSpanRegex.Replace(value, "").Trim(),
				timeParserResult.Value));
		}
	}
}