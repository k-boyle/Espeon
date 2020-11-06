using NUnit.Framework;
using System;

namespace Espeon.Test {
    public class MathExTests {
        [TestCase(4, 2, 2)]
        [TestCase(2, 2, 1)]
        [TestCase(5, 2, 3)]
        [TestCase(0, 2, 0)]
        public void TestIntCeilingDivisionGivesCorrectResult(int a, int b, int expected) {
            Assert.AreEqual(expected, MathEx.CeilingDivision(a, b));
        }
        
        [Test]
        public void TestIntCeilingDivisionThrowsOnNegativeNumberator() {
            Assert.Throws<NotSupportedException>(() => MathEx.CeilingDivision(-1, 1));
        }
        
        [Test]
        public void TestIntCeilingDivisionThrowsOnNegativeDenominator() {
            Assert.Throws<NotSupportedException>(() => MathEx.CeilingDivision(1, -1));
        }
        
        [Test]
        public void TestIntCeilingDivisionThrowsOnDivideByZero() {
            Assert.Throws<DivideByZeroException>(() => MathEx.CeilingDivision(1, 0));
        }
        
        [TestCase(4, 2, 2)]
        [TestCase(2, 2, 1)]
        [TestCase(5, 2, 3)]
        [TestCase(0, 2, 0)]
        public void TestLongCeilingDivisionGivesCorrectResult(long a, long b, int expected) {
            Assert.AreEqual(expected, MathEx.CeilingDivision(a, b));
        }
        
        [Test]
        public void TestLongCeilingDivisionThrowsOnNegativeNumberator() {
            Assert.Throws<NotSupportedException>(() => MathEx.CeilingDivision(-1L, 1L));
        }
        
        [Test]
        public void TestLongCeilingDivisionThrowsOnNegativeDenominator() {
            Assert.Throws<NotSupportedException>(() => MathEx.CeilingDivision(1L, -1L));
        }
        
        [Test]
        public void TestLongCeilingDivisionThrowsOnDivideByZero() {
            Assert.Throws<DivideByZeroException>(() => MathEx.CeilingDivision(1L, 0L));
        }
    }
}