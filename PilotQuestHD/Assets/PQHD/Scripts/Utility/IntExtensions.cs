using UnityEngine;

namespace PQHD
{
    public class IntExtensions
    {
        /// <summary>
        /// Modulo that supports negatives
        /// </summary>
        public static int Modulo(int dividend, int divisor)
        {
            int remainder = dividend % divisor;
            return (remainder < 0) ? remainder + divisor : remainder;
        }
    }
}
