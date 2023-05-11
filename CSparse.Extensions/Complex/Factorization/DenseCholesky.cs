
namespace CSparse.Complex.Factorization
{
    using CSparse.Factorization;
    using CSparse.Properties;
    using CSparse.Storage;
    using System;
    using System.Numerics;

    /// <summary>
    /// Cholesky factorization for a symmetric, positive definite matrix A = L*L'.
    /// </summary>
    /// <remarks>
    /// Only the lower triangular part of A is accessed during factorization.
    /// </remarks>
    public class DenseCholesky : ISolver<Complex>
    {
        private readonly int size;
        private readonly DenseColumnMajorStorage<Complex> L;

        /// <summary>
        /// Gets the number of rows and columns.
        /// </summary>
        public int Size => size;

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
        public static DenseCholesky Create(DenseColumnMajorStorage<Complex> matrix)
        {
            var chol = new DenseCholesky(matrix.RowCount);

            chol.Factorize(matrix);

            return chol;
        }

        /// <summary>
        /// Compute the Cholesky factorization of given matrix.
        /// </summary>
        /// <param name="matrix">The matrix to factorize.</param>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not positive definite.</exception>
        public void Factorize(DenseColumnMajorStorage<Complex> matrix)
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
        public Complex Determinant()
        {
            var values = L.Values;

            int length = size * size;

            double det = 1.0;

            for (int i = 0; i < length; i += size + 1)
            {
                det *= values[i].Real;
            }

            return new Complex(det * det, 0.0);
        }

        /// <summary>
        /// Solves a system of linear equations <b>Ax = b</b>.
        /// </summary>
        /// <param name="input">The right hand side vector <b>b</b>.</param>
        /// <param name="result">The left hand side vector <b>x</b>.</param>
        public void Solve(Complex[] input, Complex[] result)
        {
            input.CopyTo(result, 0);

            // solve L*y=b storing y in x
            DenseSolverHelper.SolveLower(size, L.Values, result);

            // solve L^T*x=y
            DenseSolverHelper.SolveLowerTranspose(size, L.Values, result);
        }

        /// <summary>
        /// Solves a system of linear equations <b>AX = B</b>.
        /// </summary>
        /// <param name="input">The right hand side matrix <b>B</b>.</param>
        /// <param name="result">The left hand side matrix <b>X</b>.</param>
        public void Solve(DenseColumnMajorStorage<Complex> input, DenseColumnMajorStorage<Complex> result)
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

            var C = new Complex[size];

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

        /// <summary>
        /// Compute the inverse using the current Cholesky factorization.
        /// </summary>
        /// <param name="target">The target matrix containing the inverse on output.</param>
        public void Inverse(DenseMatrix target)
        {
            if (target.RowCount != size || target.ColumnCount != size)
            {
                throw new ArgumentException(Resources.MatrixDimensions);
            }

            DoInvert(target.Values);
        }

        private void DoInvert(Complex[] a)
        {
            int n = size;

            // Make temp array class member?
            var temp = new Complex[n];

            var t = L.Values;

            for (int j = 0; j < n; j++)
            {
                // Unit vector.
                temp[j] = Complex.One;

                DenseSolverHelper.SolveLowerTransposeCholesky(n, t, temp);
                DenseSolverHelper.SolveLowerCholesky(n, t, temp);

                for (int i = 0; i < n; i++)
                {
                    a[i * n + j] = temp[i];

                    // Clear vector.
                    temp[i] = Complex.Zero;
                }
            }
        }

        private void DoFactorize(int n, Complex[] a)
        {
            // Diagonal entries of Hermitian matrices are real.
            double diag, invdiag = 0.0;

            for (int i = 0; i < n; i++)
            {
                int nxi = i * n;

                for (int j = i; j < n; j++)
                {
                    Complex sum = a[nxi + j];

                    int nxj = j * n;

                    for (int k = i - 1; k >= 0; k--)
                    {
                        sum -= Complex.Conjugate(a[nxi + k]) * a[nxj + k];
                    }

                    if (i == j)
                    {
                        if (sum.Real <= 0.0 || sum.Imaginary != 0.0)
                        {
                            throw new ArgumentException(Resources.MatrixPositiveDefinite);
                        }

                        diag = Math.Sqrt(sum.Real);

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
                    a[nxi + j] = Complex.Zero;
                }
            }
        }
    }
}
