using NUnit.Framework;

namespace Espeon.Test {
    public class StringJoinerTests {
        [TestCase(", ", "a, b, c")]
        [TestCase("", "abc")]
        [TestCase("\n", "a\nb\nc")]
        public void TestJoinerToStringGivesExpectedString(string seperator, string expected) {
            var joiner = new StringJoiner(seperator);
            joiner.Append("a");
            joiner.Append("b");
            joiner.Append("c");
            Assert.AreEqual(expected, joiner.ToString());
        }
        
        [Test]
        public void TestJoinerClearClearsString() {
            var joiner = new StringJoiner("");
            joiner.Append("a");
            joiner.Clear();
            Assert.AreEqual(string.Empty, joiner.ToString());
        }
        
        [Test]
        public void TestJoinerDoesntThrowsOnAppendIfDefaultCtor() {
            var joiner = new StringJoiner();
            Assert.DoesNotThrow(() => joiner.Append("a"));
        }
        
        [Test]
        public void TestJoinerDoesntThrowsOnClearIfDefaultCtor() {
            var joiner = new StringJoiner();
            Assert.DoesNotThrow(() => joiner.Clear());
        }
        
        [Test]
        public void TestJoinerDoesntThrowsOnToStringIfDefaultCtor() {
            var joiner = new StringJoiner();
            Assert.DoesNotThrow(() => joiner.ToString());
        }
    }
}