
namespace CSparse.Tests.Storage
{
    using CSparse.Double;
    using NUnit.Framework;
    using System.Linq;

    public class CompressedColumnStorageTests
    {
        [Test]
        public void TestCountPredicate()
        {
            var A = SparseMatrix.OfRowMajor(3, 3, new double[]
            {
                1.0, 0.0, 1.0,
                0.0, 2.0, 0.5,
                1.0, 0.5, 3.0
            });

            // Only 2 entries should be returned (zeros shouldn't be in the CSC storage).
            Assert.AreEqual(2, A.Count((i, j, a) => a < 1.0));

            var D = SparseMatrix.OfRowMajor(3, 3, new double[]
            {
                1.0, 0.0, 0.0,
                0.0, 2.0, 0.0,
                0.0, 0.0, 3.0
            });

            // Check for non-diagonal entries.
            Assert.AreEqual(0, D.Count((i, j, a) => i != j));
        }

        [Test]
        public void TestEnumerateIndexedPredicate()
        {
            var A = SparseMatrix.OfRowMajor(3, 2, new double[]
            {
                1.0, 0.0,
                0.0, 2.0,
                1.0, 0.5
            });

            var actual = A.EnumerateIndexed((i, j, a) => a < 1.0).ToArray();

            Assert.AreEqual(1, actual.Length);

            Assert.AreEqual(2, actual[0].Item1);
            Assert.AreEqual(1, actual[0].Item2);
            Assert.AreEqual(0.5, actual[0].Item3);
        }
    }
}
