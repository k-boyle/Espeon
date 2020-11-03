using System;
using System.Collections.Generic;

namespace Espeon
{
    public class TimeSpanParser {
        private readonly IReadOnlyDictionary<string, TimeUnit> _timeUnitByStr;

        public TimeSpanParser(IDictionary<string, TimeUnit> timeUnitByStr) {
            this._timeUnitByStr = new Dictionary<string, TimeUnit>(timeUnitByStr);
        }

        public bool TryParseIn(string input, out TimeSpan timeSpan) {
            timeSpan = TimeSpan.Zero;

            if (input.Length < 2) {
                return false;
            }

            var asSpan = input.AsSpan();

            for (int i = 0; i < asSpan.Length;) {
                if (char.IsWhiteSpace(asSpan[i])) {
                    i++;
                    continue;
                }

                var digitLength = 0;
                for (; i < asSpan.Length && (char.IsDigit(asSpan[i]) || asSpan[i] == '.') ; i++, digitLength++);

                if (digitLength == 0) {
                    i++;
                    continue;
                }

                if (i == asSpan.Length) {
                    break;
                }

                var whiteSpaces = 0;
                for (; i < asSpan.Length && char.IsWhiteSpace(asSpan[i]); i++, whiteSpaces++);

                var suffixLength = 0;
                for (; i < asSpan.Length && char.IsLetter(asSpan[i]); i++, suffixLength++);

                if (suffixLength == 0) {
                    continue;
                }

                var suffix = new string(asSpan.Slice(i - suffixLength, suffixLength));
                var parseLength = suffixLength + digitLength + whiteSpaces;
                var startIndex = i - parseLength;
                if (this._timeUnitByStr.TryGetValue(suffix, out var unit)
                        && double.TryParse(asSpan.Slice(startIndex, digitLength), out var duration)) {
                    timeSpan = timeSpan.Add(TimeSpan.FromSeconds(duration * (int) unit));
                }
            }

            return timeSpan > TimeSpan.Zero;
        }
    }
}