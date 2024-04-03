
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
            var A = SparseMatrix.OfRowMajor(3, 3,
            [
                1.0, 0.0, 1.0,
                0.0, 2.0, 0.5,
                1.0, 0.5, 3.0
            ]);

            // Only 2 entries should be returned (zeros shouldn't be in the CSC storage).
            Assert.That(A.Count((i, j, a) => a < 1.0), Is.EqualTo(2));

            var D = SparseMatrix.OfRowMajor(3, 3,
            [
                1.0, 0.0, 0.0,
                0.0, 2.0, 0.0,
                0.0, 0.0, 3.0
            ]);

            // Check for non-diagonal entries.
            Assert.That(D.Count((i, j, a) => i != j), Is.EqualTo(0));
        }

        [Test]
        public void TestAnyPredicate()
        {
            var A = SparseMatrix.OfRowMajor(3, 3,
            [
                1.0, 0.0, 0.0,
                0.0, 2.0, 0.0,
                1.0, 0.0, 3.0
            ]);

            // Test if matrix is lower triangular by checking whether there
            // is any entry above the diagonal.
            Assert.That(A.Any((i, j, a) => i < j), Is.False);
        }

        [Test]
        public void TestIsUpper()
        {
            var A = SparseMatrix.OfRowMajor(3, 3,
            [
                1.0, 0.0, 1.0,
                0.0, 2.0, 0.0,
                0.0, 0.0, 3.0
            ]);

            Assert.That(A.IsUpper(), Is.True);
            Assert.That(A.IsUpper(true), Is.False);

            A = SparseMatrix.OfRowMajor(3, 3,
            [
                0.0, 2.0, 1.0,
                0.0, 0.0, 2.0,
                0.0, 0.0, 0.0
            ]);

            Assert.That(A.IsUpper(true), Is.True);
            
            A = SparseMatrix.OfRowMajor(3, 3,
            [
                1.0, 0.0, 0.0,
                0.0, 2.0, 0.0,
                1.0, 0.0, 3.0
            ]);

            Assert.That(A.IsUpper(), Is.False);
        }

        [Test]
        public void TestIsLower()
        {
            var A = SparseMatrix.OfRowMajor(3, 3,
            [
                1.0, 0.0, 0.0,
                0.0, 2.0, 0.0,
                1.0, 0.0, 3.0
            ]);

            Assert.That(A.IsLower(), Is.True);
            Assert.That(A.IsLower(true), Is.False);

            A = SparseMatrix.OfRowMajor(3, 3,
            [
                0.0, 0.0, 0.0,
                2.0, 0.0, 0.0,
                1.0, 2.0, 0.0
            ]);

            Assert.That(A.IsLower(true), Is.True);
            
            A = SparseMatrix.OfRowMajor(3, 3,
            [
                1.0, 0.0, 1.0,
                0.0, 2.0, 0.0,
                0.0, 0.0, 3.0
            ]);

            Assert.That(A.IsLower(), Is.False);
        }

        [Test]
        public void TestEnumerateIndexedPredicate()
        {
            var A = SparseMatrix.OfRowMajor(3, 2,
            [
                1.0, 0.0,
                0.0, 2.0,
                1.0, 0.5
            ]);

            var actual = A.EnumerateIndexed((i, j, a) => a < 1.0).ToArray();

            Assert.That(actual.Length, Is.EqualTo(1));

            Assert.That(actual[0].Item1, Is.EqualTo(2));
            Assert.That(actual[0].Item2, Is.EqualTo(1));
            Assert.That(actual[0].Item3, Is.EqualTo(0.5));
        }

        [Test]
        public void TestSubMatrixSym()
        {
            // Test 1

            var A = SparseMatrix.OfRowMajor(5, 5,
            [
                1.1, 1.2, 0.0, 1.4, 1.5,
                1.2, 2.2, 2.3, 0.0, 2.5,
                0.0, 2.3, 3.3, 3.4, 0.0,
                1.4, 0.0, 3.4, 4.4, 4.5,
                1.5, 2.5, 0.0, 4.5, 5.5
            ]);

            var actual = A.SubMatrix([1, 2, 3]);

            var expected = SparseMatrix.OfRowMajor(3, 3,
            [
                2.2, 2.3, 0.0,
                2.3, 3.3, 3.4,
                0.0, 3.4, 4.4
            ]);

            Assert.That(actual.Equals(expected), Is.True, "test 1");

            // Test 2

            A = SparseMatrix.OfRowMajor(6, 6,
            [
                0.0, 1.0, 0.0, 0.0, 0.0, 0.0,
                1.0, 0.0, 2.0, 0.0, 0.0, 0.0,
                0.0, 2.0, 0.0, 3.0, 0.0, 0.0,
                0.0, 0.0, 3.0, 0.0, 4.0, 0.0,
                0.0, 0.0, 0.0, 4.0, 0.0, 5.0,
                0.0, 0.0, 0.0, 0.0, 5.0, 0.0
            ]);

            actual = A.SubMatrix([0, 2, 4]);

            expected = SparseMatrix.OfRowMajor(3, 3,
            [
                0.0, 0.0, 0.0,
                0.0, 0.0, 0.0,
                0.0, 0.0, 0.0
            ]);

            Assert.That(actual.Equals(expected), Is.True, "test 2");

            // Test 3

            actual = A.SubMatrix([0, 1, 4, 5]);

            expected = SparseMatrix.OfRowMajor(4, 4,
            [
                0.0, 1.0, 0.0, 0.0,
                1.0, 0.0, 0.0, 0.0,
                0.0, 0.0, 0.0, 5.0,
                0.0, 0.0, 5.0, 0.0
            ]);

            Assert.That(actual.Equals(expected), Is.True, "test 3");

            // Test 4

            A = SparseMatrix.OfRowMajor(5, 5,
            [
                1.1, 0.0, 0.0, 0.0, 0.0,
                1.2, 2.2, 0.0, 0.0, 0.0,
                0.0, 2.3, 3.3, 0.0, 0.0,
                1.4, 0.0, 3.4, 4.4, 0.0,
                1.5, 2.5, 0.0, 4.5, 5.5
            ]);

            actual = A.SubMatrix([1, 2, 3]);

            expected = SparseMatrix.OfRowMajor(3, 3,
            [
                2.2, 0.0, 0.0,
                2.3, 3.3, 0.0,
                0.0, 3.4, 4.4
            ]);

            Assert.That(actual.Equals(expected), Is.True, "test 4");
        }

        [Test]
        public void TestSubMatrix()
        {
            var A = SparseMatrix.OfRowMajor(4, 4,
            [
                1.1, 1.2, 0.0, 1.4,
                2.1, 2.2, 2.3, 0.0,
                0.0, 3.2, 3.3, 3.4,
                4.1, 0.0, 4.3, 4.4
            ]);

            // Test 1: row and column subset in order.

            var actual = A.SubMatrix([0, 1], [2, 3]);

            var expected = SparseMatrix.OfRowMajor(2, 2,
            [
                0.0, 1.4,
                2.3, 0.0
            ]);

            Assert.That(actual.Equals(expected), Is.True, "test 1");

            // Test 2: row subset reversed and column subset in order.

            actual = A.SubMatrix([1, 0], [2, 3]);

            expected = SparseMatrix.OfRowMajor(2, 2,
            [
                2.3, 0.0,
                0.0, 1.4
            ]);

            Assert.That(actual.Equals(expected), Is.True, "test 2");

            // Test 3: row subset in order and column subset in reversed order.

            actual = A.SubMatrix([0, 1], [3, 2]);

            expected = SparseMatrix.OfRowMajor(2, 2,
            [
                1.4, 0.0,
                0.0, 2.3
            ]);

            Assert.That(actual.Equals(expected), Is.True, "test 3");

            // Test 4: all rows and column subset in order.

            int[] ALL = null;

            actual = A.SubMatrix(ALL, [1, 2]);

            expected = SparseMatrix.OfRowMajor(4, 2,
            [
                1.2, 0.0,
                2.2, 2.3,
                3.2, 3.3,
                0.0, 4.3
            ]);

            Assert.That(actual.Equals(expected), Is.True, "test 4");

            // Test 5: row subset in order and all columns.

            actual = A.SubMatrix([1, 2], ALL);

            expected = SparseMatrix.OfRowMajor(2, 4,
            [
                2.1, 2.2, 2.3, 0.0,
                0.0, 3.2, 3.3, 3.4
            ]);

            Assert.That(actual.Equals(expected), Is.True, "test 5");
        }

        [Test]
        public void TestEliminateSymmetric()
        {
            var actual = SparseMatrix.OfRowMajor(5, 5,
            [
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2
            ]);

            var expected = SparseMatrix.OfRowMajor(5, 5,
            [
                2, 0, 2, 2, 0,
                0, 1, 0, 0, 0,
                2, 0, 2, 2, 0,
                2, 0, 2, 2, 0,
                0, 0, 0, 0, 1
            ]);

            actual.EliminateSymmetric([1, 4]);

            Assert.That(actual.Equals(expected), Is.True);
        }
    }
}
