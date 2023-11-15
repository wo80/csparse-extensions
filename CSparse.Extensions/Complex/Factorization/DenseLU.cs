
namespace CSparse.Complex.Factorization
{
    using CSparse.Factorization;
    using CSparse.Properties;
    using CSparse.Storage;
    using System;
    using System.Numerics;

    /// <summary>
    /// LU factorization for a general, square matrix A = L*U.
    /// </summary>
    public class DenseLU : ISolver<Complex>
    {
        /// <summary>
        /// Compute the LU factorization of given matrix.
        /// </summary>
        /// <param name="matrix">The matrix to factorize.</param>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        public static DenseLU Create(DenseColumnMajorStorage<Complex> matrix)
        {
            var lu = new DenseLU(matrix.RowCount, matrix.ColumnCount);

            lu.Factorize(matrix);

            return lu;
        }

        private readonly int rows;
        private readonly int columns;

        private DenseColumnMajorStorage<Complex> LU;

        // Row permutation (partial pivoting).
        private int[] perm;

        // Sign of the permutation (number of row interchanges even or odd).
        private int sign;

        private Complex[] temp;

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseCholesky"/> class.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        public DenseLU(int rows, int columns)
        {
            this.rows = rows;
            this.columns = rows;

            if (rows != columns)
            {
                throw new ArgumentException(Resources.MatrixSquare);
            }

            LU = new DenseMatrix(rows, columns);
            perm = new int[rows];
            temp = new Complex[rows];
        }

        /// <summary>
        /// Compute the LU factorization of given matrix.
        /// </summary>
        /// <param name="matrix">The matrix to factorize.</param>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        public void Factorize(DenseColumnMajorStorage<Complex> matrix)
        {
            if (matrix.RowCount != rows || matrix.ColumnCount != columns)
            {
                throw new ArgumentException(Resources.MatrixSquare);
            }

            sign = 1;

            CopyTranspose(rows, columns, matrix.Values, LU.Values);

            DoFactorize(rows, columns, LU.Values);
        }

        private void CopyTranspose(int rows, int columns, Complex[] source, Complex[] target)
        {
            for (int i = 0; i < rows; i++)
            {
                int nxi = i * columns;

                for (int j = 0; j < columns; j++)
                {
                    target[j * columns +  i] = source[nxi + j];
                }
            }
        }

        /// <summary>
        /// Determines if the decomposed matrix is singular.
        /// </summary>
        /// <returns>Return true if singular, false otherwise.</returns>
        public bool IsSingular(double eps = Constants.MachineEpsilon)
        {
            var values = LU.Values;

            for (int i = 0; i < rows; i++)
            {
                if (Complex.Abs(values[i * columns + i]) < eps)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the determinant of the matrix.
        /// </summary>
        public Complex Determinant()
        {
            Complex det = sign;

            var values = LU.Values;

            int total = rows * columns;

            for (int i = 0; i < total; i += columns + 1)
            {
                det *= values[i];
            }

            return det;
        }

        /// <inheritdoc/>
        public void Solve(Complex[] input, Complex[] result)
        {
            Solve(input.AsSpan(), result.AsSpan());
        }

        /// <inheritdoc/>
        public void Solve(ReadOnlySpan<Complex> input, Span<Complex> result)
        {
            if (input.Length < rows)
            {
                throw new ArgumentException(Resources.MatrixDimensions, nameof(input));
            }

            if (result.Length != columns)
            {
                throw new ArgumentException(Resources.MatrixDimensions, nameof(result));
            }

            input.CopyTo(result);

            DoSolve(result);
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

            if (input.RowCount != rows)
            {
                throw new ArgumentException(Resources.MatrixDimensions);
            }

            var C = new Complex[rows];

            for (int j = 0; j < columns; j++)
            {
                input.Column(j, C);

                DoSolve(C.AsSpan());

                result.SetColumn(j, C);
            }
        }

        /// <summary>
        /// Compute the inverse using the current LU factorization.
        /// </summary>
        /// <param name="target">The target matrix containing the inverse on output.</param>
        public void Inverse(DenseMatrix target)
        {
            if (target.RowCount != rows || target.ColumnCount != columns)
            {
                throw new ArgumentException(Resources.MatrixDimensions);
            }

            DoInvert(target.Values);
        }

        private void DoInvert(Complex[] a)
        {
            int n = columns;

            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < n; i++) temp[i] = i == j ? 1.0 : 0.0;

                DoSolve(temp);

                int nxj = j * n;

                // Set column j of inverse.
                for (int i = 0; i < n; i++) a[nxj + i] = temp[i];
            }
        }

        private void DoSolve(Span<Complex> result)
        {
            var values = LU.Values;

            // Solve L*Y = B
            int ii = 0;

            for (int i = 0; i < columns; i++)
            {
                int ip = perm[i];
                Complex sum = result[ip];
                result[ip] = result[i];
                if (ii != 0)
                {
                    // for( int j = ii-1; j < i; j++ )
                    //    sum -= values[i * n + j] * vv[j];
                    int index = i * columns + ii - 1;
                    for (int j = ii - 1; j < i; j++)
                        sum -= values[index++] * result[j];
                }
                else if (sum != 0.0)
                {
                    ii = i + 1;
                }

                result[i] = sum;
            }

            // Solve U*X = Y;
            DenseSolverHelper.SolveUpper(columns, values, result);
        }

        private void DoFactorize(int rows, int columns, Complex[] values)
        {
            // NOTE: this implementation expects row major order!

            Complex[] colj = temp;

            for (int j = 0; j < columns; j++)
            {
                // Make a copy of the column to avoid cache jumping issues.
                for (int i = 0; i < rows; i++)
                {
                    colj[i] = values[i * columns + j];
                }

                // Apply previous transformations.
                for (int i = 0; i < rows; i++)
                {
                    int rowIndex = i * columns;

                    // Most of the time is spent in the following dot product.
                    int kmax = i < j ? i : j;
                    Complex s = 0.0;
                    for (int k = 0; k < kmax; k++)
                    {
                        s += values[rowIndex + k] * colj[k];
                    }

                    values[rowIndex + j] = colj[i] -= s;
                }

                // Find pivot and exchange if necessary.
                int p = j;
                double max = Complex.Abs(colj[p]);
                for (int i = j + 1; i < rows; i++)
                {
                    double v = Complex.Abs(colj[i]);
                    if (v > max)
                    {
                        p = i;
                        max = v;
                    }
                }

                if (p != j)
                {
                    // Swap the rows.
                    int nxp = p * columns;
                    int nxj = j * columns;
                    int end = nxp + columns;

                    for (; nxp < end; nxp++, nxj++)
                    {
                        Complex t = values[nxp];
                        values[nxp] = values[nxj];
                        values[nxj] = t;
                    }

                    sign = -sign;
                }

                perm[j] = p;

                // Compute multipliers.
                if (j < rows)
                {
                    Complex lujj = values[j * columns + j];
                    if (lujj != 0)
                    {
                        for (int i = j + 1; i < rows; i++)
                        {
                            values[i * columns + j] /= lujj;
                        }
                    }
                }
            }
        }
    }
}