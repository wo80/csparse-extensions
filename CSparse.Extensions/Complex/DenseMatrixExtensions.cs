
namespace CSparse.Complex
{
    using CSparse.Complex.Factorization;
    using CSparse.Storage;
    using System.Numerics;

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
        public static DenseVector Multiply(this DenseColumnMajorStorage<Complex> matrix, DenseVector<Complex> x)
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
        public static DenseVector TransposeMultiply(this DenseColumnMajorStorage<Complex> matrix, DenseVector<Complex> x)
        {
            var target = new DenseVector(matrix.ColumnCount);

            matrix.TransposeMultiply(x.Values, target.Values);

            return target;
        }

        /// <summary>
        /// Solves a system of linear equations Ax = b (uses the <see cref="DenseLU"/> factorization).
        /// </summary>
        /// <param name="matrix">This matrix.</param>
        /// <param name="input">Right hand side b.</param>
        /// <returns>Solution vector x.</returns>
        public static DenseVector Solve(this DenseColumnMajorStorage<Complex> matrix, DenseVector<Complex> input)
        {
            var result = new DenseVector(matrix.RowCount);

            DenseLU.Create(matrix).Solve(input.Values, result.Values);

            return result;
        }

        /// <summary>
        /// Solves a system of linear equations Ax = b (uses the <see cref="DenseLU"/> factorization).
        /// </summary>
        /// <param name="matrix">This matrix.</param>
        /// <param name="input">Right hand side b.</param>
        /// <param name="result">Solution vector x.</param>
        public static void Solve(this DenseColumnMajorStorage<Complex> matrix, DenseVector<Complex> input, DenseVector<Complex> result)
        {
            DenseLU.Create(matrix).Solve(input.Values, result.Values);
        }

        /// <summary>
        /// Solves a system of linear equations AX = B (uses the <see cref="DenseLU"/> factorization).
        /// </summary>
        /// <param name="matrix">This matrix.</param>
        /// <param name="input">Right hand side dense matrix B.</param>
        /// <returns>Dense matrix containing the solution X.</returns>
        public static DenseMatrix Solve(this DenseColumnMajorStorage<Complex> matrix, DenseColumnMajorStorage<Complex> input)
        {
            var result = new DenseMatrix(matrix.RowCount, matrix.ColumnCount);

            DenseLU.Create(matrix).Solve(input, result);

            return result;
        }

        /// <summary>
        /// Solves a system of linear equations AX = B (uses the <see cref="DenseLU"/> factorization).
        /// </summary>
        /// <param name="matrix">This matrix.</param>
        /// <param name="input">Right hand side dense matrix B.</param>
        /// <param name="result">Dense matrix containing the solution X.</param>
        public static void Solve(this DenseColumnMajorStorage<Complex> matrix, DenseColumnMajorStorage<Complex> input, DenseColumnMajorStorage<Complex> result)
        {
            DenseLU.Create(matrix).Solve(input, result);
        }

        /// <summary>
        /// Returns the inverse of the matrix (uses the <see cref="DenseLU"/> factorization).
        /// </summary>
        /// <param name="matrix">This matrix.</param>
        /// <returns>The inverse of the matrix.</returns>
        public static DenseMatrix Inverse(this DenseColumnMajorStorage<Complex> matrix)
        {
            var result = new DenseMatrix(matrix.RowCount, matrix.ColumnCount);

            DenseLU.Create(matrix).Inverse(result);

            return result;
        }

        /// <summary>
        /// Returns the inverse of the matrix (uses the <see cref="DenseLU"/> factorization).
        /// </summary>
        /// <param name="matrix">This matrix.</param>
        /// <param name="target">The target matrix containing the inverse on return.</param>
        public static void Inverse(this DenseColumnMajorStorage<Complex> matrix, DenseColumnMajorStorage<Complex> target)
        {
            DenseLU.Create(matrix).Inverse(target);
        }

        /// <summary>
        /// Returns the determinant of the matrix (uses the <see cref="DenseLU"/> factorization).
        /// </summary>
        /// <param name="matrix">This matrix.</param>
        /// <returns>The determinant of the matrix.</returns>
        public static Complex Determinant(this DenseColumnMajorStorage<Complex> matrix)
        {
            return DenseLU.Create(matrix).Determinant();
        }
    }
}
