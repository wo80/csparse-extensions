
namespace CSparse.Tests.Double.Factorization
{
    using CSparse.Double;
    using CSparse.Double.Factorization;
    using NUnit.Framework;

    [DefaultFloatingPointTolerance(1e-12)]
    public class DenseLUTests
    {
        internal static DenseMatrix GetMatrix()
        {
            return DenseMatrix.OfRowMajor(3, 3, new double[]
            {
                4.0, -1.0,  0.5,
                2.0,  3.0, -1.0,
                1.5, -2.0,  2.0,
            }) as DenseMatrix;
        }

        [Test]
        public void TestSolve()
        {
            var A = GetMatrix();

            var solver = DenseLU.Create(A);

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

            var solver = DenseLU.Create(A);

            Assert.AreEqual(17.25, solver.Determinant());
        }

        [Test]
        public void TestInverse()
        {
            var A = GetMatrix();

            var inv = new DenseMatrix(A.RowCount, A.ColumnCount);

            var solver = DenseLU.Create(A);

            solver.Inverse(inv);

            var expected = DenseMatrix.OfRowMajor(3, 3, new double[]
            {
                 0.2318840579710, 0.0579710144927, -0.0289855072464,
                -0.3188405797101, 0.4202898550725,  0.2898550724638,
                -0.4927536231884, 0.3768115942029,  0.8115942028986
            });

            Assert.IsTrue(inv.Equals(expected, 1e-12));

            var eye = CreateDense.Eye(A.RowCount);

            Assert.IsTrue(eye.Equals(A.Multiply(inv), 1e-12));
        }
    }
}
