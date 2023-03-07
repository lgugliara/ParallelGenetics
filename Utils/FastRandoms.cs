using System;
using System.Linq;
using System.Threading;

namespace ParallelGenetics.Utils
{
    public class FastRandoms : IRandomizator
    {
        private static readonly FastRandom _globalRandom = new FastRandom(DateTime.Now.Millisecond);
        private static readonly object _globalLock = new object();
        private static int? _seed;
        private static ThreadLocal<FastRandom> _threadRandom = new ThreadLocal<FastRandom>(NewRandom);

        private static FastRandom NewRandom()
        {
            lock (_globalLock)
            {
                return new FastRandom(_seed ?? _globalRandom.Next(0, int.MaxValue));
            }
        }

        private static FastRandom Instance { get { return _threadRandom.Value; } }

        public static void ResetSeed(int? seed)
        {
            _seed = seed;
            _threadRandom = new ThreadLocal<FastRandom>(NewRandom);
        }

        public int GetInt(int min, int max)
        {
            return Instance.Next(min, max);
        }

        public float GetFloat()
        {
            return (float)Instance.NextDouble();
        }

        public double GetDouble()
        {
            return Instance.NextDouble();
        }

        public int[] GetInts(int length, int min, int max)
        {
            var ints = new int[length];

            for (int i = 0; i < length; i++)
            {
                ints[i] = GetInt(min, max);
            }

            return ints;
        }

        public int[] GetUniqueInts(int length, int min, int max)
        {
            var diff = max - min;

            if (diff < length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(length),
                    $"The length is {length}, but the possible unique values between {min} (inclusive) and {max} (exclusive) are {diff}.");
            }

            var orderedValues = Enumerable.Range(min, diff).ToList();
            var ints = new int[length];

            for (int i = 0; i < length; i++)
            {
                var removeIndex = GetInt(0, orderedValues.Count);
                ints[i] = orderedValues[removeIndex];
                orderedValues.RemoveAt(removeIndex);
            }

            return ints;
        }

        public float GetFloat(float min, float max)
        {
            return min + ((max - min) * GetFloat());
        }

        public double GetDouble(double min, double max)
        {
            return min + ((max - min) * GetDouble());
        }

        internal class FastRandom
        {
            // The +1 ensures NextDouble doesn't generate 1.0
            const double REAL_UNIT_INT = 1.0 / ((double)int.MaxValue + 1.0);
            const double REAL_UNIT_UINT = 1.0 / ((double)uint.MaxValue + 1.0);
            const uint Y = 842502087, Z = 3579807591, W = 273326509;
            uint x, y, z, w;

            #region Constructors
            /// <summary>
            /// Initialises a new instance using an int value as seed.
            /// This constructor signature is provided to maintain compatibility with
            /// System.Random
            /// </summary>
            public FastRandom(int seed)
            {
                Reinitialize(seed);
            }

            #endregion

            #region Public Methods [Reinitialisation]

            /// <summary>
            /// Reinitialises using an int value as a seed.
            /// </summary>
            /// <param name="seed"></param>
            public void Reinitialize(int seed)
            {
                // The only stipulation stated for the xorshift RNG is that at least one of
                // the seeds x,y,z,w is non-zero. We fulfill that requirement by only allowing
                // resetting of the x seed
                x = (uint)seed;
                y = Y;
                z = Z;
                w = W;
            }

            #endregion

            #region Public Methods [System.Random functionally equivalent methods]        
            /// <summary>
            /// Generates a random int over the range lowerBound to upperBound-1, and not including upperBound.
            /// upperBound must be >= lowerBound. lowerBound may be negative.
            /// </summary>
            /// <param name="lowerBound"></param>
            /// <param name="upperBound"></param>
            /// <returns></returns>
            public int Next(int lowerBound, int upperBound)
            {
                if (lowerBound > upperBound)
                    throw new ArgumentOutOfRangeException("upperBound", upperBound, "upperBound must be >=lowerBound");

                uint t = (x ^ (x << 11));
                x = y; y = z; z = w;

                // The explicit int cast before the first multiplication gives better performance.
                // See comments in NextDouble.
                int range = upperBound - lowerBound;
                if (range < 0)
                {   // If range is <0 then an overflow has occured and must resort to using long integer arithmetic instead (slower).
                    // We also must use all 32 bits of precision, instead of the normal 31, which again is slower.    
                    return lowerBound + (int)((REAL_UNIT_UINT * (double)(w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))) * (double)((long)upperBound - (long)lowerBound));
                }

                // 31 bits of precision will suffice if range<=int.MaxValue. This allows us to cast to an int and gain
                // a little more performance.
                return lowerBound + (int)((REAL_UNIT_INT * (double)(int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))))) * (double)range);
            }

            /// <summary>
            /// Generates a random double. Values returned are from 0.0 up to but not including 1.0.
            /// </summary>
            /// <returns></returns>
            public double NextDouble()
            {
                uint t = (x ^ (x << 11));
                x = y; y = z; z = w;

                // Here we can gain a 2x speed improvement by generating a value that can be cast to 
                // an int instead of the more easily available uint. If we then explicitly cast to an 
                // int the compiler will then cast the int to a double to perform the multiplication, 
                // this final cast is a lot faster than casting from a uint to a double. The extra cast
                // to an int is very fast (the allocated bits remain the same) and so the overall effect 
                // of the extra cast is a significant performance improvement.
                //
                // Also note that the loss of one bit of precision is equivalent to what occurs within 
                // System.Random.
                return (REAL_UNIT_INT * (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))));
            }
            #endregion
        }
    }

    public interface IRandomizator
    {
        public int GetInt(int min, int max);
        public int[] GetInts(int length, int min, int max);
        public int[] GetUniqueInts(int length, int min, int max);
        public float GetFloat();
        public double GetDouble();
        public float GetFloat(float min, float max);
        public double GetDouble(double min, double max);
    }
}