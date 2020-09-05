
namespace CSparse.Double
{
    using System;

    /// <summary>
    /// Create sparse matrices.
    /// </summary>
    public static class CreateDense
    {
        private const int RANDOM_SEED = 357801;

        /// <summary>
        /// Create a dense identity matrix.
        /// </summary>
        /// <param name="size">The size of the matrix.</param>
        /// <returns>Sparse identity matrix.</returns>
        public static DenseMatrix Eye(int size)
        {
            var C = new DenseMatrix(size, size);
            var values = C.Values;

            for (int i = 0; i < size; i++)
            {
                values[i * size + i] = 1.0;
            }

            return C;
        }

        /// <summary>
        /// Create a random dense matrix.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <returns>Random dense matrix.</returns>
        public static DenseMatrix Random(int rows, int columns)
        {
            return Random(rows, columns, new Random(RANDOM_SEED));
        }

        /// <summary>
        /// Create a random dense matrix.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="random">The random source.</param>
        /// <returns>Random dense matrix.</returns>
        public static DenseMatrix Random(int rows, int columns, Random random)
        {
            var C = new DenseMatrix(rows, columns);

            var values = C.Values;

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = random.NextDouble();
            }

            return C;
        }

        /// <summary>
        /// Create a random symmetric dense matrix.
        /// </summary>
        /// <param name="size">The size of the matrix.</param>
        /// <param name="definite">If true, the matrix will be positive semi-definite.</param>
        /// <returns>Random dense matrix.</returns>
        public static DenseMatrix RandomSymmetric(int size, bool definite)
        {
            return RandomSymmetric(size, definite, new Random(RANDOM_SEED));
        }

        /// <summary>
        /// Create a random symmetric dense matrix.
        /// </summary>
        /// <param name="size">The size of the matrix.</param>
        /// <param name="definite">If true, the matrix will be positive semi-definite.</param>
        /// <param name="random">The random source.</param>
        /// <returns>Random dense matrix.</returns>
        public static DenseMatrix RandomSymmetric(int size, bool definite, Random random)
        {
            var C = new DenseMatrix(size, size);

            var norm = new double[size];

            var values = C.Values;

            for (int i = 0; i < size; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    var value = random.NextDouble();

                    norm[i] += Math.Abs(value);
                    norm[j] += Math.Abs(value);

                    values[i * size + j] += value;
                    values[j * size + i] += value;
                }
            }

            // Fill diagonal.
            for (int i = 0; i < size; i++)
            {
                double value = random.NextDouble();

                if (definite)
                {
                    // Make the matrix diagonally dominant.
                    value = (value + 1.0) * norm[i];
                }

                values[i * size + i] += value;
            }

            return C;
        }
    }
}
