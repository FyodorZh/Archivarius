using System;
using Archivarius.DataModels.Compressed;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public class DataModels_LongArray
    {
        [Test]
        public void Test_LongArray()
        {
            Check([], 8);
            Check([1], 8);
            Check([1,2], 8);
            Check([1,2,3,4,5], 1);
            Check([0, long.MinValue, long.MaxValue], 8);
            Check([-1 * 10000, -2 * 10000, -3 * 10000], 1);
            Check([-127, 128, 0], 2);
        }

        private void Check(long[] array, int expectedSizePerRecord)
        {
            LongArray a = new LongArray(array);
            Assert.That(a.GetSizePerRecord(), Is.EqualTo(expectedSizePerRecord));
            var clone = a.Copy()!;
            Assert.That(clone.ValuesUnsafeToModify, Is.EquivalentTo(array));
        }
    }
}