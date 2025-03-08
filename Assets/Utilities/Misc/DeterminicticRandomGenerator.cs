namespace Volpi.Entertaiment.SDK.Utilities
{
    public class DeterministicRandomGenerator
    {
        private const long Modulus = 2147483648; 
        private const long Multiplier = 1103515245;
        private const long Increment = 12345;

        private long _currentSeed;

        public DeterministicRandomGenerator(long seed)
        {
            _currentSeed = seed;
        }

        public int Next()
        {
            _currentSeed = (Multiplier * _currentSeed + Increment) % Modulus;
            return (int)_currentSeed;
        }

        public int Next(int min, int max)
        {
            return min + (Next() % (max - min));
        }

        public float NextFloat()
        {
            return (float)Next() / Modulus;
        }

        public void SetSeed(long seed)
        {
            _currentSeed = seed;
        }
    }
}