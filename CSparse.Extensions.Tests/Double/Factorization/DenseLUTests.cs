
namespace CSparse.Tests.Double.Factorization
{
    using CSparse.Double;
    using CSparse.Double.Factorization;
    using NUnit.Framework;

    [DefaultFloatingPointTolerance(1e-12)]
    public class DenseLUTests
    {
        [Test]
        public void TestSolve()
        {
            var A = DenseMatrix.OfRowMajor(3, 3, new double[]
            {
                 4.0, -1.0,  0.5,
                -1.0,  4.0, -1.0,
                 0.5, -1.0,  4.0,
            });

            var lu = DenseLU.Create(A);

            var x = Vector.Create(3, 1.0);
            var b = Vector.Create(3, 0.0);
            var r = Vector.Create(3, 0.0);

            A.Multiply(x, b);

            lu.Solve(b, r);

            CollectionAssert.AreEqual(x, r);
        }

        [Test]
        public void TestDeterminant()
        {
            var A = DenseMatrix.OfRowMajor(3, 3, new double[]
            {
                 4.0, -1.0,  0.5,
                -1.0,  4.0, -1.0,
                 0.5, -1.0,  4.0,
            });

            var lu = DenseLU.Create(A);

            Assert.AreEqual(56.0, lu.Determinant());
        }
    }
}
