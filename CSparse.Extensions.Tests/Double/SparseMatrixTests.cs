namespace CSparse.Tests.Double
{
    using CSparse.Double;
    using NUnit.Framework;

    public class SparseMatrixTests
    {
        [Test]
        public void TestAddDiagonal()
        {
            var diag = new double[] { 1.0, 1.0 };

            // Test 1: square, full diag

            var A = SparseMatrix.OfRowMajor(2, 2, new double[]
            {
                1.0, 1.0,
                0.0, 1.0
            });

            A.AddDiagonal(diag);

            Assert.That(A.At(0, 0), Is.EqualTo(2.0));
            Assert.That(A.At(1, 1), Is.EqualTo(2.0));

            // Test 2: square, missing diag

            A = SparseMatrix.OfRowMajor(2, 2, new double[]
            {
                0.0, 1.0,
                1.0, 0.0
            });

            A.AddDiagonal(diag);

            Assert.That(A.At(0, 0), Is.EqualTo(1.0));
            Assert.That(A.At(1, 1), Is.EqualTo(1.0));

            // Test 3: columns > rows, full diag

            A = SparseMatrix.OfRowMajor(2, 3, new double[]
            {
                1.0, 0.0, 0.5,
                0.0, 1.0, 0.5
            });

            A.AddDiagonal(diag);

            Assert.That(A.At(0, 0), Is.EqualTo(2.0));
            Assert.That(A.At(1, 1), Is.EqualTo(2.0));
            Assert.That(A.At(1, 2), Is.EqualTo(0.5));

            // Test 4: columns > rows, missing diag

            A = SparseMatrix.OfRowMajor(2, 3, new double[]
            {
                0.0, 1.0, 0.5,
                1.0, 0.0, 0.5
            });

            A.AddDiagonal(diag);

            Assert.That(A.At(0, 0), Is.EqualTo(1.0));
            Assert.That(A.At(1, 1), Is.EqualTo(1.0));
            Assert.That(A.At(1, 2), Is.EqualTo(0.5));

            // Test 5: columns < rows, full diag

            A = SparseMatrix.OfRowMajor(3, 2, new double[]
            {
                1.0, 0.0,
                0.0, 1.0,
                0.5, 0.5
            });

            A.AddDiagonal(diag);

            Assert.That(A.At(0, 0), Is.EqualTo(2.0));
            Assert.That(A.At(1, 1), Is.EqualTo(2.0));
            Assert.That(A.At(2, 1), Is.EqualTo(0.5));

            // Test 6: columns < rows, missing diag

            A = SparseMatrix.OfRowMajor(3, 2, new double[]
            {
                0.0, 1.0,
                1.0, 0.0,
                0.5, 0.5
            });

            A.AddDiagonal(diag);

            Assert.That(A.At(0, 0), Is.EqualTo(1.0));
            Assert.That(A.At(1, 1), Is.EqualTo(1.0));
            Assert.That(A.At(2, 1), Is.EqualTo(0.5));

            // Test 7: explicit zeros

            diag = new double[] { 0.0, 0.0 };

            A = SparseMatrix.OfRowMajor(2, 2, new double[]
            {
                1.0, 1.0,
                1.0, 0.0
            });

            Assert.That(A.NonZerosCount, Is.EqualTo(3));

            A.AddDiagonal(diag);

            Assert.That(A.NonZerosCount, Is.EqualTo(4));
        }

        [Test]
        public void TestKroneckerProduct()
        {
            var A = SparseMatrix.OfRowMajor(2, 2, new double[]
            {
                2.0, 1.0,
                0.0, 2.0
            });

            var B = SparseMatrix.OfRowMajor(2, 3, new double[]
            {
                0.5, 1.0, 0.5,
                0.0, 0.5, 0.0
            });

            var C = A.KroneckerProduct(B);

            Assert.That(C.RowCount, Is.EqualTo(A.RowCount * B.RowCount));
            Assert.That(C.ColumnCount, Is.EqualTo(A.ColumnCount * B.ColumnCount));

            var expected = SparseMatrix.OfRowMajor(4, 6, new double[]
            {
                1.0, 2.0, 1.0, 0.5, 1.0, 0.5,
                0.0, 1.0, 0.0, 0.0, 0.5, 0.0,
                0.0, 0.0, 0.0, 1.0, 2.0, 1.0,
                0.0, 0.0, 0.0, 0.0, 1.0, 0.0
            });

            Assert.That(expected.Equals(C), Is.True);
        }
    }
}