using System;

namespace Espeon {
    public static class MathEx {
        public static int CeilingDivision(int a, int b) {
            if (a < 0) {
                throw new NotSupportedException("Numerator must be >= 0");
            }
            
            switch (b) {
                case 0:
                    throw new DivideByZeroException("Denominator must be > 0");

                case < 0:
                    throw new NotSupportedException("Denominator must be > 0");
            }

            if (a == 0) {
                return 0;
            }
            
            return (a + b - 1) / b;
        }
        
        public static long CeilingDivision(long a, long b) {
            if (a < 0) {
                throw new NotSupportedException("Numerator must be >= 0");
            }
            
            switch (b) {
                case 0:
                    throw new DivideByZeroException("Denominator must be > 0");

                case < 0:
                    throw new NotSupportedException("Denominator must be > 0");
            }

            if (a == 0) {
                return 0;
            }
            
            return (a + b - 1) / b;
        }
    }
}