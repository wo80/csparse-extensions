
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

        [Test]
        public void TestSubMatrix()
        {
            var A = SparseMatrix.OfRowMajor(4, 4, new double[]
            {
                1.1, 1.2, 0.0, 1.4,
                2.1, 2.2, 2.3, 0.0,
                0.0, 3.2, 3.3, 3.4,
                4.1, 0.0, 4.3, 4.4
            });

            // Test 1: row and column subset in order.

            var actual = A.SubMatrix(new[] { 0, 1 }, new[] { 2, 3 });

            var expected = SparseMatrix.OfRowMajor(2, 2, new double[]
            {
                0.0, 1.4,
                2.3, 0.0
            });

            Assert.IsTrue(actual.Equals(expected), "test 1");

            // Test 2: row subset reversed and column subset in order.

            actual = A.SubMatrix(new[] { 1, 0 }, new[] { 2, 3 });

            expected = SparseMatrix.OfRowMajor(2, 2, new double[]
            {
                2.3, 0.0,
                0.0, 1.4
            });

            Assert.IsTrue(actual.Equals(expected), "test 2");

            // Test 3: row subset in order and column subset in reversed order.

            actual = A.SubMatrix(new[] { 0, 1 }, new[] { 3, 2 });

            expected = SparseMatrix.OfRowMajor(2, 2, new double[]
            {
                1.4, 0.0,
                0.0, 2.3
            });

            Assert.IsTrue(actual.Equals(expected), "test 3");

            // Test 4: all rows and column subset in order.

            actual = A.SubMatrix(null, new[] { 1, 2 });

            expected = SparseMatrix.OfRowMajor(4, 2, new double[]
            {
                1.2, 0.0,
                2.2, 2.3,
                3.2, 3.3,
                0.0, 4.3
            });

            Assert.IsTrue(actual.Equals(expected), "test 4");

            // Test 5: row subset in order and all columns.

            actual = A.SubMatrix(new[] { 1, 2 }, null);

            expected = SparseMatrix.OfRowMajor(2, 4, new double[]
            {
                2.1, 2.2, 2.3, 0.0,
                0.0, 3.2, 3.3, 3.4
            });

            Assert.IsTrue(actual.Equals(expected), "test 5");
        }

        [Test]
        public void TestEliminateSymmetric()
        {
            var actual = SparseMatrix.OfRowMajor(5, 5, new double[]
            {
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2
            });

            var expected = SparseMatrix.OfRowMajor(5, 5, new double[]
            {
                2, 0, 2, 2, 0,
                0, 1, 0, 0, 0,
                2, 0, 2, 2, 0,
                2, 0, 2, 2, 0,
                0, 0, 0, 0, 1
            });

            actual.EliminateSymmetric(new[] { 1, 4 });

            Assert.IsTrue(actual.Equals(expected));
        }
    }
}
