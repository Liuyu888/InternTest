using System;
using System.Security.Cryptography;

namespace OnCallScheduler
{
    /// <summary>
    /// Strong (but slow) random generator that relies on RNG Crypto Provider to generate random numbers
    /// </summary>
    /// <remarks>
    /// This class is used to ensure better random number distribution and avoid cyclical schedule generation that has no way to improve if given more time
    /// </remarks>
    internal class StrongRandom
    {
        /// <summary>
        /// Reference to crypto service provider that powers strong random implementation
        /// </summary>
        private RNGCryptoServiceProvider _cryptoServiceProvider;

        /// <summary>
        /// Buffer of bytes pre-allocated to reduce number of RNG calls
        /// </summary>
        private byte[] _preAllocatedBuffer;

        /// <summary>
        /// Position inside pre-allocated buffer
        /// </summary>
        private int _preAllocatedBufferPosition;

        /// <summary>
        /// Default constructor
        /// </summary>
        internal StrongRandom()
        {
            // Construct
            _cryptoServiceProvider = new RNGCryptoServiceProvider();

            // Pre-allocate buffer
            _preAllocatedBuffer = new byte[2500 * 4];

            // Position in the buffer, start with at the end of the buffer to cause next request to request a buffer
            _preAllocatedBufferPosition = _preAllocatedBuffer.Length;
        }

        /// <summary>
        /// Generate a number in the given range
        /// </summary>
        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        /// <param name="minValue">The inclusive lower bound of the random number returned. </param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue. </param>
        /// <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue; that is, the range of return values includes minValue but not maxValue. If minValue equals maxValue, minValue is returned.</returns>
        internal int Next(int minValue, int maxValue)
        {
            // Safety checks
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue", "minValue is greater than maxValue");
            }
            else if (minValue == maxValue)
            {
                return minValue;
            }

            // Generate next random integer number
            int int32Value = GetNextInt32();

            // Get where in the range from Int32.Min to Int32.Max this value falls
            double Int32Ratio = ((double)int32Value - (double)Int32.MinValue) / ((double)Int32.MaxValue - (double)Int32.MinValue);

            // Calculate the value in the range given on input
            // "maxValue - 1" because exclusive
            // Using "Round" instead of cast to ensure higher fractional parts have higher probability of rouding to the boundary value
            return minValue + (int)Math.Round((((double)(maxValue - 1) - (double)minValue) * Int32Ratio));
        }

        /// <summary>
        /// Return the next integer number out of the preallocated buffer or request new buffer if depleated
        /// </summary>
        /// <returns>Next random integer number.</returns>
        private int GetNextInt32()
        {
            // We need to coordinate on this so synchronize access
            lock (this)
            {
                // Can we return next integer out of existing buffer?
                if (_preAllocatedBufferPosition + 4 > _preAllocatedBuffer.Length)
                {
                    // Request new buffer
                    _cryptoServiceProvider.GetBytes(_preAllocatedBuffer);

                    // Reset position
                    _preAllocatedBufferPosition = 0;
                }

                // Get next integer
                int int32Value = BitConverter.ToInt32(_preAllocatedBuffer, _preAllocatedBufferPosition);

                // Adjust position
                _preAllocatedBufferPosition += 4;

                // Return generated integer
                return int32Value;
            }
        }
    }
}
