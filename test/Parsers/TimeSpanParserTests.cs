using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Espeon.Test {
    public class TimeSpanParserTests {
        [TestCaseSource(nameof(StringsAndExpectedResults))]
        public void TestTryParseIn(string input, bool expectedResult, TimeSpan expectedTimeSpan) {
            var parser = new TimeSpanParser(new Dictionary<string, TimeSpanParser.TimeUnit> {
                ["s"] = TimeSpanParser.TimeUnit.SECOND,
                ["m"] = TimeSpanParser.TimeUnit.MINUTE,
                ["h"] = TimeSpanParser.TimeUnit.HOUR,
                ["d"] = TimeSpanParser.TimeUnit.DAY,
                ["mth"] = TimeSpanParser.TimeUnit.MONTH,
                ["yr"] = TimeSpanParser.TimeUnit.YEAR
            });
            var actualResult = parser.TryParseIn(input, out var actualTimeSpan);
            Assert.AreEqual(expectedResult, actualResult);
            Assert.AreEqual(expectedTimeSpan, actualTimeSpan);
        }

        private static object[][] StringsAndExpectedResults() {
            return new [] {
                new object[] { "", false, TimeSpan.Zero },
                new object[] { "pepowhatif", false, TimeSpan.Zero },
                new object[] { "5pepowhatif", false, TimeSpan.Zero },
                new object[] { "1", false, TimeSpan.Zero },
                new object[] { "0s", false, TimeSpan.Zero },
                new object[] { "1s", true, TimeSpan.FromSeconds(1) },
                new object[] { "1m", true, TimeSpan.FromMinutes(1) },
                new object[] { "1h", true, TimeSpan.FromHours(1) },
                new object[] { "1d", true, TimeSpan.FromDays(1) },
                new object[] { "1mth", true, TimeSpan.FromSeconds((int) TimeSpanParser.TimeUnit.MONTH) },
                new object[] { "1yr", true, TimeSpan.FromSeconds((int) TimeSpanParser.TimeUnit.YEAR) },
                new object[] { "5m", true, TimeSpan.FromMinutes(5) },
                new object[] { "1m 2s", true, TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(2)) },
                new object[] { "1m2s", true, TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(2)) },
                new object[] { "1 m  2     s", true, TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(2)) },
                new object[] { "pepowhatif 1m", true, TimeSpan.FromMinutes(1) },
                new object[] { "1m pepowhatif", true, TimeSpan.FromMinutes(1) },
                new object[] { "1.0s", true, TimeSpan.FromSeconds(1) },
                new object[] { "0.5m", true, TimeSpan.FromSeconds(30) },
                new object[] { "0.5s", true, TimeSpan.FromMilliseconds(500) },
                new object[] { "1 .5s", true, TimeSpan.FromMilliseconds(500) },
                new object[] { "1. 5s", true, TimeSpan.FromSeconds(5) },
                new object[] { "1h pepowhatif 5m", true, TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(5)) },
            };
        }
    }
}