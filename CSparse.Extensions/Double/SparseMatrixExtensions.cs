
namespace CSparse.Double
{
    using CSparse.Properties;
    using CSparse.Storage;
    using System;

    /// <summary>
    /// Sparse matrix extension methods.
    /// </summary>
    public static class SparseMatrixExtensions
    {
        /// <summary>
        /// Adds a diagonal to this matrix A = A + Diag.
        /// </summary>
        /// <param name="matrix">The sparse matrix.</param>
        /// <param name="diag">Array containing the matrix diagonal.</param>
        public static void AddDiagonal(this CompressedColumnStorage<double> matrix, double[] diag)
        {
            AddDiagonal(matrix, diag, matrix);
        }

        /// <summary>
        /// Adds a diagonal matrix to a general sparse matrix B = A + Diag.
        /// </summary>
        /// <param name="matrix">The sparse matrix.</param>
        /// <param name="diag">Array containing the matrix diagonal.</param>
        /// <param name="result">The resulting sparse matrix.</param>
        /// <remarks>
        /// The <paramref name="result"/> matrix may be expanded slightly to allow for additions of
        /// nonzero elements to previously non-existing diagonals.
        /// </remarks>
        public static void AddDiagonal(this CompressedColumnStorage<double> matrix, double[] diag, CompressedColumnStorage<double> result)
        {
            int rows = matrix.RowCount;
            int columns = matrix.ColumnCount;

            var ax = matrix.Values;
            var ap = matrix.ColumnPointers;
            var ai = matrix.RowIndices;

            var bx = result.Values;
            var bi = result.RowIndices;
            var bp = result.ColumnPointers;

            // Copy int arrays into result data structure if required.
            if (!ReferenceEquals(matrix, result))
            {
                if (result.ColumnCount != columns || result.RowCount != rows)
                {
                    throw new ArgumentException(Resources.MatrixDimensions);
                }

                Array.Copy(ap, bp, columns + 1);
                Array.Copy(ai, bi, ap[columns]);
                Array.Copy(ax, bx, ap[columns]);
            }

            // Get positions of diagonal elements in data structure.
            var diagind = matrix.FindDiagonalIndices();

            // Count number of holes in diagonal and add diag(*) elements to
            // valid diagonal entries.
            int icount = 0;

            // Support non-square matrices.
            int size = Math.Min(rows, columns);

            for (int j = 0; j < size; j++)
            {
                if (diagind[j] < 0)
                {
                    icount++;
                }
                else
                {
                    bx[diagind[j]] = ax[diagind[j]] + diag[j];
                }
            }

            // If no diagonal elements to insert, return.
            if (icount == 0)
            {
                return;
            }

            // Shift the nonzero elements if needed, to allow for created
            // diagonal elements.
            int k0 = bp[columns] + icount;

            // Resize storage accordingly.
            if (bi.Length < k0 || bx.Length < k0)
            {
                Array.Resize(ref bi, k0);
                Array.Resize(ref bx, k0);
            }

            int columnStart, columnEnd;
            bool test;

            // Copy columns backward.
            for (int i = columns - 1; i >= 0; i--)
            {
                // Go through column i.
                columnStart = bp[i];
                columnEnd = bp[i + 1];

                bp[i + 1] = k0;
                test = i < size && diagind[i] < 0;

                for (int k = columnEnd - 1; k >= columnStart; k--)
                {
                    int j = bi[k];
                    if (test && j < i)
                    {
                        test = false;
                        k0--;
                        bx[k0] = diag[i];
                        bi[k0] = i;
                        diagind[i] = k0;
                    }
                    k0--;
                    bx[k0] = ax[k];
                    bi[k0] = j;
                }

                // Diagonal element has not been added yet.
                if (test)
                {
                    k0--;
                    bx[k0] = diag[i];
                    bi[k0] = i;
                    diagind[i] = k0;
                }
            }

            bp[0] = k0;

            // Update storage references.
            result.RowIndices = bi;
            result.Values = bx;
        }

        /// <summary>
        /// Computes the Kronecker product of this matrix with given matrix.
        /// </summary>
        /// <param name="matrix">The sparse matrix.</param>
        /// <param name="other">The other matrix.</param>
        /// <returns>Kronecker product.</returns>
        public static SparseMatrix KroneckerProduct(this CompressedColumnStorage<double> matrix, CompressedColumnStorage<double> other)
        {
            var ap = matrix.ColumnPointers;
            var ai = matrix.RowIndices;
            var ax = matrix.Values;

            var bp = other.ColumnPointers;
            var bi = other.RowIndices;
            var bx = other.Values;

            int colsA = matrix.ColumnCount;
            int colsB = other.ColumnCount;

            var counts = new int[colsA * colsB];

            int k = 0;

            // Count non-zeros in each row of kron(A, B).
            for (int i = 0; i < colsA; i++)
            {
                for (int j = 0; j < colsB; j++)
                {
                    counts[k++] = (ap[i + 1] - ap[i]) * (bp[j + 1] - bp[j]);
                }
            }

            int rowsA = matrix.RowCount;
            int rowsB = other.RowCount;

            var cp = new int[colsA * colsB + 1];

            int nnz = Helper.CumulativeSum(cp, counts, counts.Length);

            var ci = new int[nnz];
            var cx = new double[nnz];

            k = 0;

            // For each column in A ...
            for (int ia = 0; ia < colsA; ia++)
            {
                // ... and each column in B ...
                for (int ib = 0; ib < colsB; ib++)
                {
                    // ... get element a_{ij}
                    for (int j = ap[ia]; j < ap[ia + 1]; j++)
                    {
                        var idx = ai[j];
                        var aij = ax[j];

                        // ... and multiply it with current column of B
                        for (int s = bp[ib]; s < bp[ib + 1]; s++)
                        {
                            ci[k] = (idx * rowsB) + bi[s];
                            cx[k] = aij * bx[s];
                            k++;
                        }
                    }
                }
            }

            return new SparseMatrix(rowsA * rowsB, colsA * colsB, cx, ci, cp);
        }
    }
}
