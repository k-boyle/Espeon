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

            for (int index = 0; index < asSpan.Length;) {
                if (OnlyWhiteSpacesRemain(asSpan, ref index)) {
                    break;
                }

                if (!HasDigits(asSpan, ref index, out var digitLength)) {
                    continue;
                }

                if (index == asSpan.Length) {
                    break;
                }

                if (!HasSuffix(asSpan, ref index, out var whiteSpaces, out var suffix)) {
                    continue;
                }

                var numberStartIndex = index - (suffix.Length + digitLength + whiteSpaces);
                if (this._timeUnitByStr.TryGetValue(suffix, out var unit)
                        && double.TryParse(asSpan.Slice(numberStartIndex, digitLength), out var duration)) {
                    timeSpan = timeSpan.Add(TimeSpan.FromSeconds(duration * (int) unit));
                }
            }

            return timeSpan > TimeSpan.Zero;
        }

        private static bool HasDigits(ReadOnlySpan<char> asSpan, ref int index, out int digitLength) {
            digitLength = 0;
            for (; index < asSpan.Length && (char.IsDigit(asSpan[index]) || asSpan[index] == '.'); index++, digitLength++) ;

            if (digitLength > 0) {
                return true;
            }

            index++;
            return false;

        }

        private static bool OnlyWhiteSpacesRemain(ReadOnlySpan<char> asSpan, ref int i) {
            for (; i < asSpan.Length && char.IsWhiteSpace(asSpan[i]); i++) ;
            return i == asSpan.Length;
        }

        private static bool HasSuffix(ReadOnlySpan<char> asSpan, ref int index, out int whiteSpaces, out string suffix) {
            whiteSpaces = 0;
            for (; index < asSpan.Length && char.IsWhiteSpace(asSpan[index]); index++, whiteSpaces++) ;

            var suffixLength = 0;
            for (; index < asSpan.Length && char.IsLetter(asSpan[index]); index++, suffixLength++) ;

            suffix = suffixLength > 0
                ? new string(asSpan.Slice(index - suffixLength, suffixLength))
                : string.Empty;
            return suffixLength != 0;
        }
    }
}