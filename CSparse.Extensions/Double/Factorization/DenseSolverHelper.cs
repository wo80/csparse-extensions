namespace CSparse.Double.Factorization
{
    /// <summary>
    /// Helper methods for solving triangular systems
    /// </summary>
    public static class DenseSolverHelper
    {
        /// <summary>
        /// Inverts a square lower triangular matrix:  L = L<sup>-1</sup>.
        /// </summary>
        /// <param name="L"></param>
        /// <param name="size"></param>
        public static void InvertLower(int size, double[] L)
        {
            for (int i = 0; i < size; i++)
            {
                double L_ii = L[i * size + i];
                for (int j = 0; j < i; j++)
                {
                    double val = 0;
                    for (int k = j; k < i; k++)
                    {
                        val += L[i * size + k] * L[k * size + j];
                    }
                    L[i * size + j] = -val / L_ii;
                }
                L[i * size + i] = 1.0 / L_ii;
            }
        }

        /// <summary>
        /// Solves for non-singular lower triangular matrices using forward substitution, b = L<sup>-1</sup>b.
        /// </summary>
        /// <param name="size">The size of the matrices.</param>
        /// <param name="L">An n by n non-singular lower triangular matrix. Not modified.</param>
        /// <param name="b">A vector of length n. Modified.</param>
        public static void SolveLower(int size, double[] L, double[] b)
        {
            for (int i = 0; i < size; i++)
            {
                double sum = b[i];
                int indexL = i * size;
                for (int k = 0; k < i; k++)
                {
                    sum -= L[indexL++] * b[k];
                }
                b[i] = sum / L[indexL];
            }
        }

        /// <summary>
        /// This is a forward substitution solver for non-singular lower triangular matrices, b = L <sup>-T</sup> b.
        /// </summary>
        /// <param name="size">The size of the matrices.</param>
        /// <param name="L">An n by n non-singular lower triangular matrix. Not modified.</param>
        /// <param name="b">A vector of length n. Modified.</param>
        public static void SolveLowerTranspose(int size, double[] L, double[] b)
        {
            for (int i = size - 1; i >= 0; i--)
            {
                double sum = b[i];
                for (int k = i + 1; k < size; k++)
                {
                    sum -= L[k * size + i] * b[k];
                }
                b[i] = sum / L[i * size + i];
            }
        }

        /// <summary>
        /// This is a forward substitution solver for non-singular upper triangular matrices, b = U<sup>-1</sup> b.
        /// </summary>
        /// <param name="size">The size of the matrices.</param>
        /// <param name="U">An n by n non-singular upper triangular matrix. Not modified.</param>
        /// <param name="b">A vector of length n. Modified.</param>
        public static void SolveUpper(int size, double[] U, double[] b)
        {
            for (int i = size - 1; i >= 0; i--)
            {
                double sum = b[i];
                int indexU = i * size + i + 1;
                for (int j = i + 1; j < size; j++)
                {
                    sum -= U[indexU++] * b[j];
                }
                b[i] = sum / U[i * size + i];
            }
        }
    }
}
