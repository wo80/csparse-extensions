
namespace CSparse.Complex
{
    using CSparse.Storage;
    using System;
    using System.Numerics;

    public class DenseVector : DenseVector<Complex>
    {
        /// <inheritdoc />
        public DenseVector(int count)
            : base(count, new Complex[count])
        {
        }

        /// <inheritdoc />
        public DenseVector(Complex[] values)
            : base(values.Length, values)
        {
        }

        /// <inheritdoc />
        public DenseVector(int count, Complex[] values)
            : base(count, values)
        {
        }

        /// <inheritdoc />
        public override void Add(DenseVector<Complex> other, DenseVector<Complex> target)
        {
            var x = other.Values;
            var y = target.Values;

            for (int i = 0; i < count; i++)
            {
                y[i] = values[i] + x[i];
            }
        }

        /// <inheritdoc />
        public override void Scale(Complex value, DenseVector<Complex> target)
        {
            Vector.Scale(count, value, values, target.Values);
        }

        /// <inheritdoc />
        public override void PointwiseMultiply(DenseVector<Complex> other, DenseVector<Complex> target)
        {
            Vector.PointwiseMultiply(count, values, other.Values, target.Values);
        }

        /// <inheritdoc />
        public override double L1Norm()
        {
            var sum = 0d;
            for (int i = 0; i < count; i++)
            {
                sum += values[i].Magnitude;
            }
            return sum;
        }

        /// <inheritdoc />
        public override double L2Norm()
        {
            return Vector.Norm(count, values);
        }

        /// <inheritdoc />
        public override double InfinityNorm()
        {
            var max = 0d;
            for (var i = 0; i < count; i++)
            {
                max = Math.Max(values[i].Magnitude, max);
            }
            return max;
        }

        /// <inheritdoc />
        public override Complex DotProduct(DenseVector<Complex> other)
        {
            return Vector.DotProduct(count, values, other.Values);
        }

        /// <inheritdoc />
        public override DenseVector<Complex> Clone()
        {
            return new DenseVector(count, Vector.Clone(values));
        }
    }
}
