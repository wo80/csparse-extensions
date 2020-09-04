
namespace CSparse.Double.Factorization
{
    using CSparse.Factorization;
    using CSparse.Properties;
    using CSparse.Solvers;
    using CSparse.Storage;
    using System;

    /// <summary>
    /// Cholesky factorization for a symmetric, positive definite matrix A = L*L'.
    /// </summary>
    /// <remarks>
    /// Only the lower triangular part of A is accessed during factorization.
    /// </remarks>
    public class DenseCholesky : ISolver<double>
    {
        /// <summary>
        /// Compute the Cholesky factorization of given matrix.
        /// </summary>
        /// <param name="matrix">The matrix to factorize.</param>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not positive definite.</exception>
        public static DenseCholesky Create(DenseColumnMajorStorage<double> matrix)
        {
            var chol = new DenseCholesky(matrix.RowCount);

            chol.Factorize(matrix);

            return chol;
        }

        private readonly int size;
        private DenseColumnMajorStorage<double> L;

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseCholesky"/> class.
        /// </summary>
        /// <param name="size"></param>
        public DenseCholesky(int size)
        {
            this.size = size;

            L = new DenseMatrix(size, size);
        }

        /// <summary>
        /// Compute the Cholesky factorization of given matrix.
        /// </summary>
        /// <param name="matrix">The matrix to factorize.</param>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not positive definite.</exception>
        public void Factorize(DenseColumnMajorStorage<double> matrix)
        {
            if (matrix.RowCount != size || matrix.ColumnCount != size)
            {
                throw new ArgumentException(Resources.MatrixSquare);
            }

            matrix.Values.CopyTo(L.Values, 0);

            DoFactorize(size, L.Values);
        }

        /// <summary>
        /// Gets the determinant of the matrix.
        /// </summary>
        public double Determinant()
        {
            var values = L.Values;

            int length = size * size;

            double det = 1.0;

            for (int i = 0; i < length; i += size + 1)
            {
                det *= values[i];
            }

            return det * det;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side vector, <b>x</b>.</param>
        public void Solve(double[] input, double[] result)
        {
            input.CopyTo(result, 0);

            // solve L*y=b storing y in x
            DenseSolverHelper.SolveLower(size, L.Values, result);

            // solve L^T*x=y
            DenseSolverHelper.SolveLowerTranspose(size, L.Values, result);
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>.
        /// </summary>
        /// <param name="input">The right hand side <see cref="DenseMatrix"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="DenseMatrix"/>, <b>X</b>.</param>
        public void Solve(DenseMatrix input, DenseMatrix result)
        {
            int columns = input.ColumnCount;

            if (result.RowCount != input.RowCount)
            {
                throw new ArgumentException(Resources.MatrixSameRowDimension);
            }

            if (result.ColumnCount != columns)
            {
                throw new ArgumentException(Resources.MatrixSameColumnDimension);
            }

            if (input.RowCount != L.RowCount)
            {
                throw new ArgumentException(Resources.MatrixDimensions);
            }

            var C = new double[size];

            for (int j = 0; j < columns; j++)
            {
                input.Column(j, C);

                // solve L*y=b storing y in x
                DenseSolverHelper.SolveLower(size, L.Values, C);

                // solve L^T*x=y
                DenseSolverHelper.SolveLowerTranspose(size, L.Values, C);

                result.SetColumn(j, C);
            }
        }

        private static void DoFactorize(int n, double[] a)
        {
            double diag, invdiag = 0.0;

            for (int i = 0; i < n; i++)
            {
                int nxi = i * n;

                for (int j = i; j < n; j++)
                {
                    double sum = a[nxi + j];

                    int nxj = j * n;

                    for (int k = i - 1; k >= 0; k--)
                    {
                        sum -= a[nxi + k] * a[nxj + k];
                    }

                    if (i == j)
                    {
                        if (sum <= 0.0)
                        {
                            throw new ArgumentException(Resources.MatrixPositiveDefinite);
                        }

                        diag = Math.Sqrt(sum);

                        a[nxi + i] = diag;

                        invdiag = 1.0 / diag;
                    }
                    else
                    {
                        a[nxj + i] = sum * invdiag;
                    }
                }
            }

            // Zero the top right corner.
            for (int i = 0; i < n; i++)
            {
                int nxi = i * n;

                for (int j = i + 1; j < n; j++)
                {
                    a[nxi + j] = 0.0;
                }
            }
        }
    }
}
