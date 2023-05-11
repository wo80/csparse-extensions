namespace CSparse.Extensions.Tests.Double
{
    using CSparse.Double;
    using NUnit.Framework;
    using System;

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

            Assert.AreEqual(4.0, a.DotProduct(b));
        }

        [Test]
        public void TestNorm()
        {
            var a = new DenseVector(new[] { 0.0, -1.0, 2.0, -3.0 });

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
    }
}
