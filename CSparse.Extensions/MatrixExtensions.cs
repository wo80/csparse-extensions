using CSparse.Storage;
using System;

namespace CSparse
{
    /// <summary>
    /// <see cref="Matrix{T}"/> extension methods.
    /// </summary>
    public static class MatrixExtensions
    {
        /// <summary>
        /// Multiplies a (m-by-n) matrix by a vector, y = A*x. 
        /// </summary>
        /// <param name="matrix">This matrix.</param>
        /// <param name="x">Vector of length n (column count).</param>
        /// <param name="target">Target vector of length m (row count).</param>
        public static void Multiply<T>(this Matrix<T> matrix, DenseVector<T> x, DenseVector<T> target)
            where T : struct, IEquatable<T>, IFormattable
        {
            matrix.Multiply(x.Values, target.Values);
        }

        /// <summary>
        /// Multiplies the transpose of a (m-by-n) matrix by a vector, y = A'*x. 
        /// </summary>
        /// <param name="matrix">This matrix.</param>
        /// <param name="x">Vector of length m (column count of A').</param>
        /// <param name="target">Target vector of length n (row count of A').</param>
        public static void TransposeMultiply<T>(this Matrix<T> matrix, DenseVector<T> x, DenseVector<T> target)
            where T : struct, IEquatable<T>, IFormattable
        {
            matrix.TransposeMultiply(x.Values, target.Values);
        }
    }
}
