
namespace CSparse.Double.Preconditioner
{
    using System;
    using CSparse.Solvers;
    using CSparse.Storage;
    using CSparse.Properties;

    /// <summary>
    /// A simple MILU(0) preconditioner.
    /// </summary>
    /// <remarks>
    /// The preconditioner does not preserve symmetry
    /// Original Fortran code by Youcef Saad (07 January 2004)
    /// </remarks>
    public sealed class MILU0 : IPreconditioner<double>
    {
        // Matrix stored in Modified Sparse Row (MSR) format containing the L and U
        // factors together.

        // The diagonal (stored in alu(0:n-1) ) is inverted. Each i-th row of the matrix
        // contains the i-th row of L (excluding the diagonal entry = 1) followed by
        // the i-th row of U.
        private readonly double[] lux;

        // The row pointers (stored in jlu(0:n) ) and column indices to off-diagonal elements.
        private readonly int[] lup;

        // Pointer to the diagonal elements in MSR storage (for faster LU solving).
        private readonly int[] diag;

        /// <summary>
        /// Gets or sets a value indicating whether to use modified or standard ILU(0).
        /// </summary>
        public bool UseModified { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="MILU0"/> class.
        /// </summary>
        /// <param name="matrix">The matrix upon which the preconditioner is based. </param>
        /// <param name="symmetric">Value indicating whether the matrix is symmetric or not.</param>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square or is not an instance of CompressedColumnStorage.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="matrix"/> has a zero pivot.</exception>
        public MILU0(Matrix<double> matrix, bool symmetric = false)
        {
            var storage = matrix as CompressedColumnStorage<double>;

            if (storage == null)
            {
                throw new ArgumentException("Expected matrix of type CompressedColumnStorage<double>.");
            }

            int n = storage.RowCount;

            // Check dimension of matrix
            if (n != storage.ColumnCount)
            {
                throw new ArgumentException(Resources.MatrixSquare, nameof(matrix));
            }

            // Code was written for CSR storage, needs update to CSC.
            var T = symmetric ? storage :  storage.Transpose();

            // Original matrix compressed sparse row storage.
            var ax = T.Values;
            var ap = T.ColumnPointers;
            var ai = T.RowIndices;

            lux = new double[ap[n] + 1];
            lup = new int[ap[n] + 1];
            diag = new int[n];

            int code = Compute(n, ax, ai, ap, lux, lup, diag, UseModified);

            if (code > -1)
            {
                throw new InvalidOperationException("Zero pivot encountered on row " + code + " during ILU process");
            }
        }

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Ax = b</b>.
        /// </summary>
        /// <param name="input">The right hand side vector b.</param>
        /// <param name="result">The left hand side vector x.</param>
        public void Apply(double[] input, double[] result)
        {
            if (lux == null)
            {
                throw new ArgumentNullException();
            }

            int n = diag.Length;

            if ((result.Length != input.Length) || (result.Length != n))
            {
                throw new ArgumentException(Resources.VectorsSameLength);
            }

            // Forward solve.
            for (int i = 0; i < n; i++)
            {
                result[i] = input[i];
                for (int k = lup[i]; k < diag[i]; k++)
                {
                    result[i] = result[i] - lux[k] * result[lup[k]];
                }
            }

            // Backward solve.
            for (int i = n - 1; i >= 0; i--)
            {
                for (int k = diag[i]; k < lup[i + 1]; k++)
                {
                    result[i] = result[i] - lux[k] * result[lup[k]];
                }
                result[i] = lux[i] * result[i];
            }
        }

        /// <summary>
        /// MILU0 is a simple milu(0) preconditioner.
        /// </summary>
        /// <param name="n">Order of the matrix.</param>
        /// <param name="ax">Matrix values in CSR format (input).</param>
        /// <param name="ai">Column indices (input).</param>
        /// <param name="ap">Row pointers (input).</param>
        /// <param name="lux">Matrix values in MSR format (output).</param>
        /// <param name="lup">Row pointers and column indices (output).</param>
        /// <param name="diag">Pointer to diagonal elements (output).</param>
        /// <param name="modified">True if the modified/MILU algorithm should be used (recommended)</param>
        /// <returns>Returns 0 on success or k > 0 if a zero pivot was encountered at step k.</returns>
        private int Compute(int n, double[] ax, int[] ai, int[] ap, double[] lux, int[] lup, int[] diag, bool modified)
        {
            var iw = new int[n];
            int i;

            // Set initial pointer value.
            int p = n + 1;
            lup[0] = p;

            // Initialize work vector.
            for (i = 0; i < n; i++)
            {
                iw[i] = -1;
            }

            // The main loop.
            for (i = 0; i < n; i++)
            {
                int pold = p;

                // Generating row i of L and U.
                int j;
                for (j = ap[i]; j < ap[i + 1]; j++)
                {
                    // Copy row i of A, JA, IA into row i of ALU, JLU (LU matrix).
                    int jcol = ai[j];

                    if (jcol == i)
                    {
                        lux[i] = ax[j];
                        iw[jcol] = i;
                        diag[i] = p;
                    }
                    else
                    {
                        lux[p] = ax[j];
                        lup[p] = ai[j];
                        iw[jcol] = p;
                        p = p + 1;
                    }
                }

                lup[i + 1] = p;

                double s = 0.0;

                int k;
                for (j = pold; j < diag[i]; j++)
                {
                    int jrow = lup[j];
                    double tl = lux[j] * lux[jrow];
                    lux[j] = tl;

                    // Perform linear combination.
                    for (k = diag[jrow]; k < lup[jrow + 1]; k++)
                    {
                        int jw = iw[lup[k]];
                        if (jw != -1)
                        {
                            lux[jw] = lux[jw] - tl * lux[k];
                        }
                        else
                        {
                            // Accumulate fill-in values.
                            s = s + tl * lux[k];
                        }
                    }
                }

                if (modified)
                {
                    lux[i] = lux[i] - s;
                }

                if (lux[i] == 0.0)
                {
                    return i;
                }

                // Invert and store diagonal element.
                lux[i] = 1.0 / lux[i];

                // Reset pointers in work array.
                iw[i] = -1;
                for (k = pold; k < p; k++)
                {
                    iw[lup[k]] = -1;
                }
            }

            return -1;
        }
    }
}
