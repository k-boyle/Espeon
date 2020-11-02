using System.Buffers;
using System.Globalization;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Espeon
{
    public class TimeSpanParser {
        public enum TimeUnit {
            SECOND = 1,
            MINUTE = 60,
            HOUR = 3600,
            DAY = 86400,
            WEEK = 604800,
            MONTH = 2628000,
            YEAR = 31536000
        }

        private readonly Dictionary<string, TimeUnit> _timeUnitByStr;

        public TimeSpanParser() {
            this._timeUnitByStr = new Dictionary<string, TimeUnit>(StringComparer.InvariantCultureIgnoreCase) {
                ["s"] = TimeUnit.SECOND,
                ["sec"] = TimeUnit.SECOND,
                ["secs"] = TimeUnit.SECOND,
                ["second"] = TimeUnit.SECOND,
                ["seconds"] = TimeUnit.SECOND,

                ["m"] = TimeUnit.MINUTE,
                ["min"] = TimeUnit.MINUTE,
                ["minute"] = TimeUnit.MINUTE,
                ["minutes"] = TimeUnit.MINUTE,

                ["h"] = TimeUnit.HOUR,
                ["hr"] = TimeUnit.HOUR,
                ["hrs"] = TimeUnit.HOUR,
                ["hour"] = TimeUnit.HOUR,
                ["hours"] = TimeUnit.HOUR,

                ["d"] = TimeUnit.DAY,
                ["day"] = TimeUnit.DAY,
                ["days"] = TimeUnit.DAY,

                ["wk"] = TimeUnit.WEEK,
                ["wks"] = TimeUnit.WEEK,
                ["week"] = TimeUnit.WEEK,
                ["weeks"] = TimeUnit.WEEK,

                ["mth"] = TimeUnit.MONTH,
                ["mths"] = TimeUnit.MONTH,
                ["month"] = TimeUnit.MONTH,
                ["months"] = TimeUnit.MONTH,

                ["y"] = TimeUnit.YEAR,
                ["yr"] = TimeUnit.YEAR,
                ["yrs"] = TimeUnit.YEAR,
                ["year"] = TimeUnit.YEAR,
                ["years"] = TimeUnit.YEAR
            };
        }

        public TimeSpanParser(Dictionary<string, TimeUnit> timeUnitByStr) {
            this._timeUnitByStr = new Dictionary<string, TimeUnit>(timeUnitByStr);
        }

        public unsafe bool TryParseIn(string input, out TimeSpan timeSpan) {
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
                for (; i < asSpan.Length && char.IsDigit(asSpan[i]) || asSpan[i] == '.' ; i++, digitLength++);

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
                if (_timeUnitByStr.TryGetValue(suffix, out var unit)
                        && double.TryParse(asSpan.Slice(startIndex, digitLength), out var duration)) {
                    timeSpan = timeSpan.Add(TimeSpan.FromSeconds(duration * (int) unit));
                }
            }

            return timeSpan > TimeSpan.Zero;
        }
    }
}