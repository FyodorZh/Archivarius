using System;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public class DataModels_DateTimeArray
    {
        [Test]
        public void Test_DateTimeArray()
        {
            Check([
                new DateTime(new DateOnly(2020, 1, 1), new TimeOnly(), DateTimeKind.Utc),
                new DateTime(new DateOnly(2020, 1, 2), new TimeOnly(), DateTimeKind.Utc),
                new DateTime(new DateOnly(2020, 1, 3), new TimeOnly(), DateTimeKind.Utc),
                new DateTime(new DateOnly(2020, 1, 4), new TimeOnly(), DateTimeKind.Utc),
                new DateTime(new DateOnly(2020, 1, 5), new TimeOnly(), DateTimeKind.Utc),
                new DateTime(new DateOnly(2020, 1, 6), new TimeOnly(), DateTimeKind.Utc),
                new DateTime(new DateOnly(2020, 1, 7), new TimeOnly(), DateTimeKind.Utc),
                new DateTime(new DateOnly(2020, 1, 8), new TimeOnly(), DateTimeKind.Utc),
                new DateTime(new DateOnly(2020, 1, 9), new TimeOnly(), DateTimeKind.Utc),
            ], 1);
            Check([
                new DateTime(new DateOnly(2020, 1, 1), new TimeOnly(), DateTimeKind.Local),
                new DateTime(new DateOnly(2020, 1, 2), new TimeOnly(), DateTimeKind.Local),
                new DateTime(new DateOnly(2020, 1, 3), new TimeOnly(), DateTimeKind.Local),
                new DateTime(new DateOnly(2020, 1, 4), new TimeOnly(), DateTimeKind.Local),
                new DateTime(new DateOnly(2020, 1, 5), new TimeOnly(), DateTimeKind.Local),
                new DateTime(new DateOnly(2020, 1, 6), new TimeOnly(), DateTimeKind.Local),
                new DateTime(new DateOnly(2020, 1, 7), new TimeOnly(), DateTimeKind.Local),
                new DateTime(new DateOnly(2020, 1, 8), new TimeOnly(), DateTimeKind.Local),
                new DateTime(new DateOnly(2020, 1, 9), new TimeOnly(), DateTimeKind.Local),
            ], 1);
            Check([
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 2),
                new DateTime(2020, 1, 3)
            ], 1);
        }

        private void Check(DateTime[] array, int expectedSizePerRecord)
        {
            DateTimeArray a = new DateTimeArray(array);
            Assert.That(a.GetSizePerRecord(), Is.EqualTo(expectedSizePerRecord));
            var clone = a.Copy()!;
            Assert.That(clone.ToArray(), Is.EquivalentTo(array));
        }
    }
}