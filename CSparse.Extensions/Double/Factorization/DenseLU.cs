
namespace CSparse.Double.Factorization
{
    using CSparse.Factorization;
    using CSparse.Properties;
    using CSparse.Storage;
    using System;

    /// <summary>
    /// LU factorization for a general, square matrix A = L*U.
    /// </summary>
    public class DenseLU : ISolver<double>
    {
        /// <summary>
        /// Compute the LU factorization of given matrix.
        /// </summary>
        /// <param name="matrix">The matrix to factorize.</param>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        public static DenseCholesky Create(DenseColumnMajorStorage<double> matrix)
        {
            var chol = new DenseCholesky(matrix.RowCount);

            chol.Factorize(matrix);

            return chol;
        }

        private readonly int rows;
        private readonly int columns;

        private DenseColumnMajorStorage<double> LU;

        // Row permutation (partial pivoting).
        private int[] perm;

        // Sign of the permutation (number of row interchanges even or odd).
        private int sign;

        private double[] temp;

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
            temp = new double[rows];
        }

        /// <summary>
        /// Compute the LU factorization of given matrix.
        /// </summary>
        /// <param name="matrix">The matrix to factorize.</param>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        public void Factorize(DenseColumnMajorStorage<double> matrix)
        {
            if (matrix.RowCount != rows || matrix.ColumnCount != columns)
            {
                throw new ArgumentException(Resources.MatrixSquare);
            }

            sign = 1;

            matrix.Values.CopyTo(LU.Values, 0);

            DoFactorize(rows, columns, LU.Values);
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
                if (Math.Abs(values[i * columns + i]) < eps)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the determinant of the matrix.
        /// </summary>
        public double Determinant()
        {
            double det = sign;

            var values = LU.Values;

            int total = rows * columns;

            for (int i = 0; i < total; i += columns + 1)
            {
                det *= values[i];
            }

            return det;
        }

        /// <inheritdoc/>
        public void Solve(double[] input, double[] result)
        {
            if (input.Length < rows)
            {
                throw new ArgumentException(Resources.MatrixDimensions, nameof(input));
            }

            if (result.Length != columns)
            {
                throw new ArgumentException(Resources.MatrixDimensions, nameof(result));
            }

            input.CopyTo(result, 0);

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

            var C = new double[rows];

            for (int j = 0; j < columns; j++)
            {
                input.Column(j, C);

                DoSolve(C);

                result.SetColumn(j, C);
            }
        }

        private void DoSolve(double[] result)
        {
            var values = LU.Values;

            // Solve L*Y = B
            int ii = 0;

            for (int i = 0; i < columns; i++)
            {
                int ip = perm[i];
                double sum = result[ip];
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

        private void DoFactorize(int rows, int columns, double[] values)
        {
            double[] colj = temp;

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
                    double s = 0.0;
                    for (int k = 0; k < kmax; k++)
                    {
                        s += values[rowIndex + k] * colj[k];
                    }

                    values[rowIndex + j] = colj[i] -= s;
                }

                // Find pivot and exchange if necessary.
                int p = j;
                double max = Math.Abs(colj[p]);
                for (int i = j + 1; i < rows; i++)
                {
                    double v = Math.Abs(colj[i]);
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
                        double t = values[nxp];
                        values[nxp] = values[nxj];
                        values[nxj] = t;
                    }

                    sign = -sign;
                }

                perm[j] = p;

                // Compute multipliers.
                if (j < rows)
                {
                    double lujj = values[j * columns + j];
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