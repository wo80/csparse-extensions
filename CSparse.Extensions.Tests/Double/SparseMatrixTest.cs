namespace CSparse.Tests.Double
{
    using CSparse.Double;
    using NUnit.Framework;

    public class SparseMatrixTests
    {
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

            Assert.AreEqual(A.RowCount * B.RowCount, C.RowCount);
            Assert.AreEqual(A.ColumnCount * B.ColumnCount, C.ColumnCount);

            var expected = SparseMatrix.OfRowMajor(4, 6, new double[]
            {
                1.0, 2.0, 1.0, 0.5, 1.0, 0.5,
                0.0, 1.0, 0.0, 0.0, 0.5, 0.0,
                0.0, 0.0, 0.0, 1.0, 2.0, 1.0,
                0.0, 0.0, 0.0, 0.0, 1.0, 0.0
            });

            Assert.IsTrue(expected.Equals(C));
        }
    }
}