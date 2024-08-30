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

            Assert.That(actual.Values, Is.EqualTo(expected.Values));
        }

        [Test]
        public void TestScale()
        {
            var a = DenseVector.Create(4, 2.0);

            var expected = DenseVector.Create(4, 1.0);
            var actual = a.Scale(0.5);

            Assert.That(actual.Values, Is.EqualTo(expected.Values));
        }

        [Test]
        public void TestDotProduct()
        {
            var a = DenseVector.Create(4, 2.0);
            var b = DenseVector.Create(4, 0.5);

            Assert.That(a.DotProduct(b), Is.EqualTo(4.0));
        }

        [Test]
        public void TestNorm()
        {
            var a = new DenseVector([0.0, -1.0, 2.0, -3.0]);

            Assert.That(a.L1Norm(), Is.EqualTo(6.0));
            Assert.That(a.L2Norm(), Is.EqualTo(Math.Sqrt(14.0)));
            Assert.That(a.InfinityNorm(), Is.EqualTo(3.0));
        }

        [Test]
        public void TestPointwiseMultiply()
        {
            var a = DenseVector.Create(3, 2.0);
            var b = DenseVector.Create(3, 0.5);

            var expected = DenseVector.Create(3, 1.0);
            var actual = a.PointwiseMultiply(b);

            Assert.That(actual.Values, Is.EqualTo(expected.Values));
        }
    }
}
