
namespace CSparse.Double
{
    using CSparse.Storage;

    /// <summary>
    /// <see cref="DenseMatrix"/> extension methods.
    /// </summary>
    public static class DenseMatrixExtensions
    {
        /// <summary>
        /// Multiplies a (m-by-n) matrix by a vector, y = A*x. 
        /// </summary>
        /// <param name="matrix">This matrix.</param>
        /// <param name="x">Vector of length n (column count).</param>
        /// <returns>Vector of length m (row count), containing the result.</returns>
        public static DenseVector Multiply(this DenseColumnMajorStorage<double> matrix, DenseVector<double> x)
        {
            var target = new DenseVector(matrix.RowCount);

            matrix.Multiply(x.Values, target.Values);

            return target;
        }

        /// <summary>
        /// Multiplies the transpose of a (m-by-n) matrix by a vector, y = A'*x. 
        /// </summary>
        /// <param name="matrix">This matrix.</param>
        /// <param name="x">Vector of length m (column count of A').</param>
        /// <returns>Vector of length n (row count of A'), containing the result.</returns>
        public static DenseVector TransposeMultiply(this DenseColumnMajorStorage<double> matrix, DenseVector<double> x)
        {
            var target = new DenseVector(matrix.ColumnCount);

            matrix.TransposeMultiply(x.Values, target.Values);

            return target;
        }
    }
}
