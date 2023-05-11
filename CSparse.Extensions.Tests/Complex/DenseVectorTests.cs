namespace CSparse.Extensions.Tests.Complex
{
    using CSparse.Complex;
    using NUnit.Framework;
    using System;
    using Complex = System.Numerics.Complex;

    public class DenseVectorTests
    {
        [Test]
        public void TestAdd()
        {
            var a = DenseVector.Create(4, 1.0);
            var b = DenseVector.Create(4, 2.0);

            var expected = DenseVector.Create(4, 3.0);
            var actual = a.Add(b);

            Assert.AreEqual(expected.Values, actual.Values);
        }

        [Test]
        public void TestScale()
        {
            var a = DenseVector.Create(4, 2.0);

            var expected = DenseVector.Create(4, 1.0);
            var actual = a.Scale(0.5);

            Assert.AreEqual(expected.Values, actual.Values);
        }

        [Test]
        public void TestDotProduct()
        {
            var a = DenseVector.Create(4, 2.0);
            var b = DenseVector.Create(4, 0.5);

            Assert.AreEqual(C(4.0), a.DotProduct(b));
        }

        [Test]
        public void TestNorm()
        {
            var a = new DenseVector(new[] { C(0.0), C(-1.0), C(2.0), C(-3.0) });

            Assert.AreEqual(6.0, a.L1Norm());
            Assert.AreEqual(Math.Sqrt(14.0), a.L2Norm());
            Assert.AreEqual(3.0, a.InfinityNorm());
        }

        [Test]
        public void TestPointwiseMultiply()
        {
            var a = DenseVector.Create(3, 2.0);
            var b = DenseVector.Create(3, 0.5);

            var expected = DenseVector.Create(3, 1.0);
            var actual = a.PointwiseMultiply(b);

            Assert.AreEqual(expected.Values, actual.Values);
        }

        private static Complex C(double a, double b = 0.0)
        {
            return new Complex(a, b);
        }
    }
}
