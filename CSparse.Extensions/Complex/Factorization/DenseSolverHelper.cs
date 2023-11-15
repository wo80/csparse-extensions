namespace CSparse.Complex.Factorization
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Helper methods for solving triangular systems
    /// </summary>
    public static class DenseSolverHelper
    {
        /// <summary>
        /// Solves for non-singular lower triangular matrices using forward substitution, b = L<sup>-1</sup>b.
        /// </summary>
        /// <param name="size">The size of the matrices.</param>
        /// <param name="L">An n by n non-singular lower triangular matrix. Not modified.</param>
        /// <param name="b">A vector of length n. Modified.</param>
        public static void SolveLower(int size, Complex[] L, Span<Complex> b)
        {
            for (int i = 0; i < size; i++)
            {
                Complex sum = b[i];
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
        public static void SolveLowerTranspose(int size, Complex[] L, Span<Complex> b)
        {
            for (int i = size - 1; i >= 0; i--)
            {
                Complex sum = b[i];
                for (int k = i + 1; k < size; k++)
                {
                    sum -= Complex.Conjugate(L[k * size + i]) * b[k];
                }
                b[i] = sum / L[i * size + i].Real;
            }
        }

        /// <summary>
        /// This is a forward substitution solver for non-singular upper triangular matrices, b = U<sup>-1</sup> b.
        /// </summary>
        /// <param name="size">The size of the matrices.</param>
        /// <param name="U">An n by n non-singular upper triangular matrix. Not modified.</param>
        /// <param name="b">A vector of length n. Modified.</param>
        public static void SolveUpper(int size, Complex[] U, Span<Complex> b)
        {
            for (int i = size - 1; i >= 0; i--)
            {
                Complex sum = b[i];
                int indexU = i * size + i + 1;
                for (int j = i + 1; j < size; j++)
                {
                    sum -= U[indexU++] * b[j];
                }
                b[i] = sum / U[i * size + i];
            }
        }

        /// <summary>
        /// Solves for non-singular lower triangular matrices with real valued
        /// diagonal elements using forward substitution.
        /// </summary>
        /// <param name="size">The size of the matrices.</param>
        /// <param name="L">An n by n non-singular lower triangular matrix. Not modified.</param>
        /// <param name="b">A vector of length n. Modified.</param>
        internal static void SolveLowerCholesky(int size, Complex[] L, Span<Complex> b)
        {
            for (int i = size - 1; i >= 0; i--)
            {
                Complex sum = b[i];

                for (int k = i + 1; k < size; k++)
                {
                    sum -= L[k * size + i] * b[k];
                }

                b[i] = sum / L[i * size + i].Real;
            }
        }

        /// <summary>
        /// Forward substitution solver for non-singular lower triangular matrices
        /// with real valued diagonal elements.
        /// </summary>
        /// <param name="size">The size of the matrices.</param>
        /// <param name="L">An n by n non-singular lower triangular matrix. Not modified.</param>
        /// <param name="b">A vector of length n. Modified.</param>
        internal static void SolveLowerTransposeCholesky(int size, Complex[] L, Span<Complex> b)
        {
            for (int i = 0; i < size; i++)
            {
                Complex sum = b[i];

                int indexL = i * size;
                for (int k = 0; k < i; k++)
                {
                    sum -= Complex.Conjugate(L[indexL++]) * b[k];
                }

                b[i] = sum / L[indexL].Real;
            }
        }
    }
}
