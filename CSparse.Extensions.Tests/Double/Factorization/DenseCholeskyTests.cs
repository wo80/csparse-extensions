
namespace CSparse.Tests.Double.Factorization
{
    using CSparse.Double;
    using CSparse.Double.Factorization;
    using NUnit.Framework;

    [DefaultFloatingPointTolerance(1e-12)]
    public class DenseCholeskyTests
    {
        private static DenseMatrix GetMatrix()
        {
            return DenseMatrix.OfRowMajor(3, 3, new double[]
            {
                 4.0, -1.0,  0.5,
                -1.0,  4.0, -1.0,
                 0.5, -1.0,  4.0,
            }) as DenseMatrix;
        }

        [Test]
        public void TestSolve()
        {
            var A = GetMatrix();

            var solver = DenseCholesky.Create(A);

            var x = Vector.Create(3, 1.0);
            var b = Vector.Create(3, 0.0);
            var r = Vector.Create(3, 0.0);

            A.Multiply(x, b);

            solver.Solve(b, r);

            CollectionAssert.AreEqual(x, r);
        }

        [Test]
        public void TestDeterminant()
        {
            var A = GetMatrix();

            var solver = DenseCholesky.Create(A);

            Assert.AreEqual(56.0, solver.Determinant());
        }

        [Test]
        public void TestInvert()
        {
            var A = GetMatrix();

            var inv = new DenseMatrix(A.RowCount, A.ColumnCount);

            var solver = DenseCholesky.Create(A);

            solver.Inverse(inv);

            var expected = DenseMatrix.OfRowMajor(3, 3, new double[]
            {
                 0.2678571428571, 0.0624999999999, -0.0178571428571,
                 0.0624999999999, 0.2812499999999,  0.0624999999999,
                -0.0178571428571, 0.0624999999999,  0.2678571428571
            });

            Assert.IsTrue(inv.Equals(expected, 1e-12));

            var eye = CreateDense.Eye(A.RowCount);

            Assert.IsTrue(eye.Equals(A.Multiply(inv), 1e-12));
        }
    }
}
