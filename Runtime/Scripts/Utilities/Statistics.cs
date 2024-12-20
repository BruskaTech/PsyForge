//Copyright (c) 2024 Columbia University (James Bruska)
//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PsyForge.Utilities {

    public class Statistics {
        /// <summary>
        /// Calculates the factorial of the number.
        /// https://stackoverflow.com/a/51740258
        /// </summary>
        /// <param name="integer"></param>
        /// <returns></returns>
        public static BigInteger Factorial(BigInteger integer) {
            if(integer < 1) return new BigInteger(1);

            BigInteger result = integer;
            for (BigInteger i = 1; i < integer; i++)
            {
                result = result * i;
            }

            return result;
        }

        /// <summary>
        /// Calculates the permutation of n and r (nPr).
        /// https://stackoverflow.com/a/51740258
        /// </summary>
        /// <param name="n"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static BigInteger Permutation(BigInteger n, BigInteger r) {
            return Factorial(n) / Factorial(n-r);
        }
        /// <summary>
        /// Calculates the combination of n and r (nCr).
        /// https://stackoverflow.com/a/51740258
        /// </summary>
        /// <param name="n"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static BigInteger Combination(BigInteger n, BigInteger r) {
            return Permutation(n, r) / Factorial(r);
        }

        /// <summary>
        /// Calculates the mathematical modulus of a number.
        /// This is not the C# remainder operator.
        /// The big difference is that the mathematical modulus is always positive.
        /// ex: -1 % 5 = 4
        /// </summary>
        /// <param name="dividend">The dividend</param>
        /// <param name="divisor">The divisor</param>
        /// <returns>The modulus of the dividend divided by the divisor.</returns>
        public static int Mod(int dividend, int divisor) {
            int remainder = dividend % divisor;
            return remainder < 0 ? remainder + divisor : remainder;
        }
        /// <summary>
        /// Calculates the mathematical modulus of a number.
        /// This is not the C# remainder operator.
        /// The big difference is that the mathematical modulus is always positive.
        /// ex: -1 % 5 = 4
        /// </summary>
        /// <param name="dividend">The dividend</param>
        /// <param name="divisor">The divisor</param>
        /// <returns>The modulus of the dividend divided by the divisor.</returns>
        public static BigInteger Mod(BigInteger dividend, BigInteger divisor) {
            BigInteger remainder = dividend % divisor;
            return remainder < 0 ? remainder + divisor : remainder;
        }

        /// <summary>
        /// Calculates the percentile of a sequence of numbers.
        /// </summary>
        /// <param name="inputSequence"></param>
        /// <param name="percentile">Value must be between 0 and 1 (inclusive)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static double Percentile(IList<int> inputSequence, double percentile) {
            if (inputSequence == null || inputSequence.Count == 0) {
                throw new ArgumentException("The sequence is empty or null.");
            } else if (percentile < 0 || percentile > 1) {
                throw new ArgumentOutOfRangeException($"The percentile ({percentile}) must be between 0 and 1 (inclusive).");
            }

            List<int> sequence = new(inputSequence);
            sequence.Sort();
            double realIndex = percentile * (sequence.Count - 1);
            int index = (int)realIndex;
            double frac = realIndex - index;

            if (index + 1 < sequence.Count)
                return sequence[index] + (frac * (sequence[index + 1] - sequence[index]));
            else
                return sequence[index];
        }
        /// <summary>
        /// Calculates the percentile of a sequence of numbers.
        /// </summary>
        /// <param name="inputSequence"></param>
        /// <param name="percentile">Value must be between 0 and 1 (inclusive)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static double Percentile(IList<double> inputSequence, double percentile) {
            if (inputSequence == null || inputSequence.Count == 0) {
                throw new ArgumentException("The sequence is empty or null.");
            } else if (percentile < 0 || percentile > 1) {
                throw new ArgumentOutOfRangeException($"The percentile ({percentile}) must be between 0 and 1 (inclusive).");
            }

            List<double> sequence = new(inputSequence);
            sequence.Sort();
            double realIndex = percentile * (sequence.Count - 1);
            int index = (int)realIndex;
            double frac = realIndex - index;

            if (index + 1 < sequence.Count)
                return sequence[index] + (frac * (sequence[index + 1] - sequence[index]));
            else
                return sequence[index];
        }
    }
}