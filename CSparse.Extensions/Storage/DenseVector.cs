
namespace CSparse.Storage
{
    using System;

    /// <summary>
    /// Base class for dense vectors.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DenseVector<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Number of elements
        /// </summary>
        protected int count;

        /// <summary>
        /// Values array.
        /// </summary>
        protected T[] values;

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector{T}"/> class.
        /// </summary>
        /// <param name="count">The vector dimension.</param>
        public DenseVector(int count)
            : this(count, new T[count])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector{T}"/> class.
        /// </summary>
        /// <param name="values">The values array (ownership is taken, no copy).</param>
        public DenseVector(T[] values)
            : this(values.Length, values)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector{T}"/> class.
        /// </summary>
        /// <param name="count">The vector dimension.</param>
        /// <param name="values">The values array (ownership is taken, no copy).</param>
        public DenseVector(int count, T[] values)
        {
            if (values.Length < count)
            {
                throw new ArgumentException("The values array must at least be of size 'count'", nameof(values));
            }

            this.count = count;
            this.values = values;
        }

        /// <summary>
        /// Gets the number of dimensions.
        /// </summary>
        public int Count => count;

        /// <summary>
        /// Gets the values array of this vector.
        /// </summary>
        public T[] Values => values;

        /// <summary>
        /// Gets or sets the value at the given <paramref name="index"/>.
        /// </summary>
        public T this[int index]
        {
            get { return values[index]; }
            set { values[index] = value; }
        }

        /// <summary>
        /// Gets the value at the given <paramref name="index"/> without range checking.
        /// </summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <returns>The value of the vector at the given <paramref name="index"/>.</returns>
        public T At(int index)
        {
            return values[index];
        }

        /// <summary>
        /// Sets the <paramref name="value"/> at the given <paramref name="index"/> without range checking.
        /// </summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <param name="value">The value to set.</param>
        public void At(int index, T value)
        {
            values[index] = value;
        }

        /// <summary>
        /// Resets all values of the vector to zero.
        /// </summary>
        public void Clear()
        {
            Array.Clear(values, 0, count);
        }

        /// <summary>
        /// Computes the sum of two vectors.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns></returns>
        public DenseVector<T> Add(DenseVector<T> other)
        {
            var result = Create(count);

            Add(other, result);

            return result;
        }

        /// <summary>
        /// Computes the sum of two vectors.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <param name="target">The target vector.</param>
        public abstract void Add(DenseVector<T> other, DenseVector<T> target);

        /// <summary>
        /// Computes a scaled version of this vector.
        /// </summary>
        /// <param name="value">The scaling factor.</param>
        /// <returns></returns>
        public DenseVector<T> Scale(T value)
        {
            var result = Create(count);

            Scale(value, result);

            return result;
        }

        /// <summary>
        /// Computes a scaled version of this vector.
        /// </summary>
        /// <param name="value">The scaling factor.</param>
        /// <param name="target">The target vector.</param>
        public abstract void Scale(T value, DenseVector<T> target);

        /// <summary>
        /// Computes the pointwise product of two vectors.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns></returns>
        public DenseVector<T> PointwiseMultiply(DenseVector<T> other)
        {
            var result = Create(count);

            PointwiseMultiply(other, result);

            return result;
        }

        /// <summary>
        /// Computes the pointwise product of two vectors.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <param name="target">The target vector.</param>
        public abstract void PointwiseMultiply(DenseVector<T> other, DenseVector<T> target);

        /// <summary>
        /// Computes the dot product of two vectors.
        /// </summary>
        /// <returns>The dot product.</returns>
        public abstract T DotProduct(DenseVector<T> other);

        /// <summary>
        /// Create a clone of this vector.
        /// </summary>
        /// <returns>The cloned vector.</returns>
        public abstract DenseVector<T> Clone();

        /// <summary>
        /// Copies the values of this vector into the target vector.
        /// </summary>
        /// <param name="target">The vector to copy elements into.</param>
        public void CopyTo(DenseVector<T> target)
        {
            CopyTo(target.Values);
        }

        /// <summary>
        /// Copies the values of this vector into the target array.
        /// </summary>
        /// <param name="target">The array to copy elements into.</param>
        public void CopyTo(T[] target)
        {
            Array.Copy(values, target, count);
        }

        /// <summary>
        /// Calculates the L1 norm of the vector, also known as Manhattan norm.
        /// </summary>
        /// <returns>The sum of the absolute values.</returns>
        public abstract double L1Norm();

        /// <summary>
        /// Calculates the L2 norm of the vector, also known as Euclidean norm.
        /// </summary>
        /// <returns>The square root of the sum of the squared values.</returns>
        public abstract double L2Norm();

        /// <summary>
        /// Calculates the infinity norm of the vector.
        /// </summary>
        /// <returns>The maximum absolute value.</returns>
        public abstract double InfinityNorm();

        /// <summary>
        /// Returns the data contained in the vector as an array.
        /// </summary>
        /// <returns>The vector data array (NOT a copy).</returns>
        public T[] ToArray() => values;

        #region Internal methods

        internal static DenseVector<T> Create(int count)
        {
            if (typeof(T) == typeof(double))
            {
                return new CSparse.Double.DenseVector(count)
                    as DenseVector<T>;
            }

            if (typeof(T) == typeof(System.Numerics.Complex))
            {
                return new CSparse.Complex.DenseVector(count)
                    as DenseVector<T>;
            }

            throw new NotSupportedException();
        }

        #endregion
    }
}
