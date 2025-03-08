using Volpi.Entertaiment.SDK.Utilities;

namespace Volpi.Entertaiment.SDK.UtilitiesEditor.Tests
{
    using NUnit.Framework;
    
    public class DeterministicRandomGeneratorTests
    {
        [Test]
        public void Next_GeneratesSameSequence_WithSameSeed()
        {
            DeterministicRandomGenerator randomGen1 = new(12345);
            DeterministicRandomGenerator randomGen2 = new(12345);

            Assert.AreEqual(randomGen1.Next(), randomGen2.Next());
            Assert.AreEqual(randomGen1.Next(), randomGen2.Next());
            Assert.AreEqual(randomGen1.Next(), randomGen2.Next());
        }

        [Test]
        public void Next_GeneratesDifferentSequence_WithDifferentSeeds()
        {
            DeterministicRandomGenerator randomGen1 = new(12345);
            DeterministicRandomGenerator randomGen2 = new(67890);

            Assert.AreNotEqual(randomGen1.Next(), randomGen2.Next());
        }

        [Test]
        public void NextFloat_GeneratesValuesBetweenZeroAndOne()
        {
            DeterministicRandomGenerator randomGen = new(12345);

            for (int i = 0; i < 1000; i++)
            {
                float randomFloat = randomGen.NextFloat();
                Assert.IsTrue(randomFloat is >= 0.0f and < 1.0f);
            }
        }

        [Test]
        public void SetSeed_ResetsRandomSequence()
        {
            DeterministicRandomGenerator randomGen = new(12345);
            int firstValue = randomGen.Next();

            randomGen.SetSeed(12345);
            int resetValue = randomGen.Next();

            Assert.AreEqual(firstValue, resetValue);
        }

        [Test]
        public void Next_GeneratesValuesWithinRange()
        {
            DeterministicRandomGenerator randomGen = new(12345);

            for (int i = 0; i < 1000; i++)
            {
                int randomValue = randomGen.Next(1, 10);
                Assert.IsTrue(randomValue is >= 1 and < 10);
            }
        }
    }
}