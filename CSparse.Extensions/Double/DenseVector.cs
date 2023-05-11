
namespace CSparse.Double
{
    using CSparse.Storage;
    using System;

    public class DenseVector : DenseVector<double>
    {
        /// <inheritdoc />
        public DenseVector(int count)
            : base(count, new double[count])
        {
        }

        /// <inheritdoc />
        public DenseVector(double[] values)
            : base(values.Length, values)
        {
        }

        /// <inheritdoc />
        public DenseVector(int count, double[] values)
            : base(count, values)
        {
        }

        /// <inheritdoc />
        public override void Add(DenseVector<double> other, DenseVector<double> target)
        {
            var x = other.Values;
            var y = target.Values;

            for (var i = 0; i < count; i++)
            {
                y[i] = values[i] + x[i];
            }
        }

        /// <inheritdoc />
        public override void Scale(double value, DenseVector<double> target)
        {
            Vector.Scale(count, value, values, target.Values);
        }

        /// <inheritdoc />
        public override void PointwiseMultiply(DenseVector<double> other, DenseVector<double> target)
        {
            Vector.PointwiseMultiply(count, values, other.Values, target.Values);
        }

        /// <inheritdoc />
        public override double L1Norm()
        {
            var sum = 0d;
            for (var i = 0; i < count; i++)
            {
                sum += Math.Abs(values[i]);
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
                max = Math.Max(Math.Abs(values[i]), max);
            }
            return max;
        }

        /// <inheritdoc />
        public override double DotProduct(DenseVector<double> other)
        {
            return Vector.DotProduct(count, values, other.Values);
        }

        /// <inheritdoc />
        public override DenseVector<double> Clone()
        {
            return new DenseVector(count, Vector.Clone(values));
        }
    }
}
