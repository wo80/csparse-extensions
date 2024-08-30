
namespace CSparse.Tests.Complex.Factorization
{
    using CSparse.Complex;
    using CSparse.Complex.Factorization;
    using NUnit.Framework;
    using System.Linq;
    using Complex = System.Numerics.Complex;

    [DefaultFloatingPointTolerance(1e-12)]
    public class DenseLUTests
    {
        internal static DenseMatrix GetMatrix()
        {
            return DenseMatrix.OfRowMajor(3, 3,
            [
                 C(4.0, 0.0), C(-1.0, 0.0), C( 0.5, 2.0),
                 C(2.0, 1.0), C( 3.0, 0.0), C(-1.0, 1.0),
                 C(1.5, 0.5), C(-2.0, 0.5), C( 2.0, 0.0),
            ]) as DenseMatrix;
        }

        [Test]
        public void TestSolve()
        {
            var A = GetMatrix();

            var solver = DenseLU.Create(A);

            var x = Vector.Create(3, Complex.One);
            var b = Vector.Create(3, Complex.Zero);
            var r = Vector.Create(3, Complex.Zero);

            A.Multiply(x, b);

            solver.Solve(b, r);

            // Comparing complex arrays doesn't respect the floating point tolerance.
            //CollectionAssert.AreEqual(x, r);

            Assert.That(r.Select(a => a.Real), Is.EqualTo(x.Select(a => a.Real)).AsCollection);
            Assert.That(r.Select(a => a.Imaginary), Is.EqualTo(x.Select(a => a.Imaginary)).AsCollection);
        }

        [Test]
        public void TestDeterminant()
        {
            var A = GetMatrix();

            var solver = DenseLU.Create(A);

            Assert.That(solver.Determinant().Real, Is.EqualTo(24.5));
        }

        [Test]
        public void TestInverse()
        {
            var A = GetMatrix();

            var inv = new DenseMatrix(A.RowCount, A.ColumnCount);

            var solver = DenseLU.Create(A);

            solver.Inverse(inv);

            var eye = CreateDense.Eye(A.RowCount);

            Assert.That(eye.Equals(A.Multiply(inv), 1e-12), Is.True);
        }

        private static Complex C(double a, double b = 0.0)
        {
            return new Complex(a, b);
        }
    }
}
