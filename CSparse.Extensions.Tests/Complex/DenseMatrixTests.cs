namespace CSparse.Extensions.Tests.Complex
{
    using CSparse.Complex;
    using CSparse.Tests.Complex.Factorization;
    using NUnit.Framework;
    using System.Linq;
    using Complex = System.Numerics.Complex;

    [DefaultFloatingPointTolerance(1e-12)]
    public class DenseMatrixTests
    {
        [Test]
        public void TestSolve()
        {
            var A = DenseLUTests.GetMatrix();

            var x = new DenseVector(Vector.Create(3, Complex.One));
            var b = new DenseVector(Vector.Create(3, Complex.Zero));
            var r = new DenseVector(Vector.Create(3, Complex.Zero));

            A.Multiply(x, b);
            A.Solve(b, r);

            // Comparing complex arrays doesn't respect the floating point tolerance.
            //CollectionAssert.AreEqual(x, r);

            CollectionAssert.AreEqual(x.Values.Select(a => a.Real), r.Values.Select(a => a.Real));
            CollectionAssert.AreEqual(x.Values.Select(a => a.Imaginary), r.Values.Select(a => a.Imaginary));
        }

        [Test]
        public void TestDeterminant()
        {
            var A = DenseLUTests.GetMatrix();

            Assert.AreEqual(24.5, A.Determinant().Real);
        }

        [Test]
        public void TestInvert()
        {
            var A = DenseLUTests.GetMatrix();

            var inv = new DenseMatrix(A.RowCount, A.ColumnCount);

            A.Inverse(inv);

            var eye = CreateDense.Eye(A.RowCount);

            Assert.IsTrue(eye.Equals(A.Multiply(inv), 1e-12));
        }
    }
}
