
namespace CSparse.Double.Preconditioner
{
    using CSparse.Properties;
    using CSparse.Solvers;
    using CSparse.Storage;
    using System;

    /// <summary>
    /// A diagonal preconditioner.
    /// </summary>
    public class Diagonal : IPreconditioner<double>
    {
        /// <summary>
        /// 
        /// </summary>
        public bool ThrowOnMissingDiagonal { get; set; }

        // The inverse of the matrix diagonal.
        double[] inverseDiagonal;

        /// <summary>
        /// Initializes a new instance of the <see cref="Diagonal"/> class.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix{T}"/> upon which this preconditioner is based.</param>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="matrix"/> has zeros on diagonal.</exception>
        public Diagonal(Matrix<double> matrix)
        {
            int rows = matrix.RowCount;

            if (rows != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.MatrixSquare, nameof(matrix));
            }

            inverseDiagonal = new double[rows];

            if (matrix is CompressedColumnStorage<double> A)
            {
                var ax = A.Values;
                var ap = A.ColumnPointers;
                var ai = A.RowIndices;

                for (int i = 0; i < rows; i++)
                {
                    // Find entry (i,i) of matrix.
                    int k = Array.BinarySearch(ai, ap[i], ap[i + 1] - ap[i], i);

                    if (k >= 0)
                    {
                        inverseDiagonal[i] = ax[k];
                    }
                }
            }
            else
            {
                for (int i = 0; i < rows; i++)
                {
                    inverseDiagonal[i] = matrix.At(i, i);
                }
            }

            for (var i = 0; i < rows; i++)
            {
                double value = inverseDiagonal[i];

                if (value == 0.0)
                {
                    if (ThrowOnMissingDiagonal)
                    {
                        throw new InvalidOperationException();
                    }

                    inverseDiagonal[i] = 1.0;
                }
                else
                {
                    inverseDiagonal[i] = 1.0 / value;
                }
            }
        }

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Ax = b</b>.
        /// </summary>
        /// <param name="input">The right hand side vector b.</param>
        /// <param name="result">The left hand side vector x.</param>
        public void Apply(double[] input, double[] result)
        {
            for (var i = 0; i < inverseDiagonal.Length; i++)
            {
                result[i] = input[i] * inverseDiagonal[i];
            }
        }
    }
}
