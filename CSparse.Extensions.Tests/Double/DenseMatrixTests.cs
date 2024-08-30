namespace CSparse.Extensions.Tests.Double
{
    using CSparse.Double;
    using CSparse.Tests.Double.Factorization;
    using NUnit.Framework;

    [DefaultFloatingPointTolerance(1e-12)]
    public class DenseMatrixTests
    {
        [Test]
        public void TestSolve()
        {
            var A = DenseLUTests.GetMatrix();

            var x = new DenseVector(Vector.Create(3, 1.0));
            var b = new DenseVector(Vector.Create(3, 0.0));
            var r = new DenseVector(Vector.Create(3, 0.0));

            A.Multiply(x, b);
            A.Solve(b, r);

            Assert.That(r.Values, Is.EqualTo(x.Values).AsCollection);
        }

        [Test]
        public void TestDeterminant()
        {
            var A = DenseLUTests.GetMatrix();

            Assert.That(A.Determinant(), Is.EqualTo(17.25));
        }

        [Test]
        public void TestInverse()
        {
            var A = DenseLUTests.GetMatrix();

            var inv = new DenseMatrix(A.RowCount, A.ColumnCount);

            A.Inverse(inv);

            var eye = CreateDense.Eye(A.RowCount);

            Assert.That(eye.Equals(A.Multiply(inv), 1e-12), Is.True);
        }
    }
}
