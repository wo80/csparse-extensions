
namespace CSparse.Tests.Complex.Factorization
{
    using CSparse.Complex;
    using CSparse.Complex.Factorization;
    using NUnit.Framework;
    using System.Linq;
    using Complex = System.Numerics.Complex;

    [DefaultFloatingPointTolerance(1e-12)]
    public class DenseCholeskyTests
    {
        private static DenseMatrix GetMatrix()
        {
            Complex v = new Complex(4.0, 0.0);
            Complex w = new Complex(1.0, 0.5);
            Complex z = new Complex(0.0, 0.5);

            return DenseMatrix.OfRowMajor(3, 3, new Complex[]
            {
                   v,    w,  z,
                 C(w),   v,  w,
                 C(z), C(w), v
            }) as DenseMatrix;
        }

        [Test]
        public void TestSolve()
        {
            var A = GetMatrix();

            var solver = DenseCholesky.Create(A);

            var x = Vector.Create(3, Complex.One);
            var b = Vector.Create(3, Complex.Zero);
            var r = Vector.Create(3, Complex.Zero);

            A.Multiply(x, b);

            solver.Solve(b, r);

            // Comparing complex arrays doesn't respect the floating point tolerance.
            //CollectionAssert.AreEqual(x, r);

            CollectionAssert.AreEqual(x.Select(a => a.Real), r.Select(a => a.Real));
            CollectionAssert.AreEqual(x.Select(a => a.Imaginary), r.Select(a => a.Imaginary));
        }

        [Test]
        public void TestDeterminant()
        {
            var A = GetMatrix();

            var solver = DenseCholesky.Create(A);

            Assert.AreEqual(54.0, solver.Determinant().Real);
        }

        [Test]
        public void TestInvert()
        {
            var A = GetMatrix();

            var inv = new DenseMatrix(A.RowCount, A.ColumnCount);

            var solver = DenseCholesky.Create(A);

            solver.Inverse(inv);

            var eye = CreateDense.Eye(A.RowCount);

            Assert.IsTrue(eye.Equals(A.Multiply(inv), 1e-12));
        }

        private static Complex C(Complex z)
        {
            return Complex.Conjugate(z);
        }
    }
}
