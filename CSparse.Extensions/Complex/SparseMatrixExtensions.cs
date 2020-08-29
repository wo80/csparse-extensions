namespace CSparse.Complex
{
    using CSparse.Storage;
    using System.Numerics;

    /// <summary>
    /// <see cref="SparseMatrix"/> extension methods.
    /// </summary>
    public static class SparseMatrixExtensions
    {
        /// <summary>
        /// Computes the Kronecker product of this matrix with given matrix.
        /// </summary>
        /// <param name="matrix">The sparse matrix.</param>
        /// <param name="other">The other matrix.</param>
        /// <returns>Kronecker product.</returns>
        public static SparseMatrix KroneckerProduct(this CompressedColumnStorage<Complex> matrix, CompressedColumnStorage<Complex> other)
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
            var cx = new Complex[nnz];

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