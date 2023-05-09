
namespace CSparse.Double
{
    using CSparse.Properties;
    using CSparse.Storage;
    using System;

    /// <summary>
    /// Create sparse matrices.
    /// </summary>
    public static class CreateSparse
    {
        private const int RANDOM_SEED = 357801;
        
        /// <summary>
        /// Create an empty sparse matrix.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="valueCount">The number of entries to allocate (default = 0).</param>
        /// <returns>Empty sparse matrix.</returns>
        public static SparseMatrix Zeros(int rows, int columns, int valueCount = 0)
        {
            var A = new SparseMatrix(rows, columns);
            
            A.ColumnPointers = new int[columns];
            A.RowIndices = new int[valueCount];
            A.Values = new double[valueCount];

            return A;
        }

        /// <summary>
        /// Create a sparse identity matrix.
        /// </summary>
        /// <param name="size">The size of the matrix.</param>
        /// <returns>Sparse identity matrix.</returns>
        public static SparseMatrix Eye(int size)
        {
            var A = new SparseMatrix(size, size, size);
            
            var ap = A.ColumnPointers;
            var ai = A.RowIndices;
            var ax = A.Values;

            for (int i = 0; i < size; i++)
            {
                ap[i] = i;
                ai[i] = i;
                ax[i] = 1.0;
            }

            ap[size] = size;

            return A;
        }

        /// <summary>
        /// Create a random sparse matrix.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="density">The density (between 0.0 and 1.0).</param>
        /// <returns>Random sparse matrix.</returns>
        public static SparseMatrix Random(int rows, int columns, double density)
        {
            return Random(rows, columns, density, rows, new Random(RANDOM_SEED));
        }

        /// <summary>
        /// Create a random sparse matrix.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="density">The density (between 0.0 and 1.0).</param>
        /// <param name="band">The bandwidth of the matrix.</param>
        /// <returns>Random sparse matrix.</returns>
        public static SparseMatrix Random(int rows, int columns, double density, int band)
        {
            return Random(rows, columns, density, band, new Random(RANDOM_SEED));
        }

        /// <summary>
        /// Create a random sparse matrix.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="density">The density (between 0.0 and 1.0).</param>
        /// <param name="band">The bandwidth of the matrix.</param>
        /// <param name="random">The random source.</param>
        /// <returns>Random sparse matrix.</returns>
        public static SparseMatrix Random(int rows, int columns, double density, int band, Random random)
        {
            if (rows < 0)
            {
                throw new ArgumentException(Resources.ValueNonNegative, nameof(rows));
            }

            if (columns < 0)
            {
                throw new ArgumentException(Resources.ValueNonNegative, nameof(columns));
            }

            if (rows == 0 || columns == 0)
            {
                return new SparseMatrix(rows, columns);
            }

            // Number of non-zeros per row.
            int nz = (int)Math.Max(columns * density, 1d);

            var C = new CoordinateStorage<double>(rows, columns, rows * nz);

            for (int i = 0; i < rows; i++)
            {
                // Make sure diagonal is set.
                C.At(i, i, 1.0);

                for (int j = 0; j < nz; j++)
                {
                    int k = Math.Min(columns - 1, (int)(random.NextDouble() * columns));

                    // TODO: implement bandwidth
                    /*
                    int k = (int)(2 * (random.NextDouble() - 0.5) * band);

                    k = Math.Min(columns - 1, Math.Max(0, i - k));
                    */

                    C.At(i, k, random.NextDouble());
                }
            }

            return SparseMatrix.OfIndexed(C) as SparseMatrix;
        }

        /// <summary>
        /// Create a random symmetric sparse matrix.
        /// </summary>
        /// <param name="size">The size of the matrix.</param>
        /// <param name="density">The density (between 0.0 and 1.0).</param>
        /// <returns>Random sparse matrix.</returns>
        public static SparseMatrix RandomSymmetric(int size, double density)
        {
            return RandomSymmetric(size, density, false, new Random(RANDOM_SEED));
        }

        /// <summary>
        /// Create a random symmetric sparse matrix.
        /// </summary>
        /// <param name="size">The size of the matrix.</param>
        /// <param name="density">The density (between 0.0 and 1.0).</param>
        /// <param name="definite">If true, the matrix will be positive semi-definite.</param>
        /// <returns>Random sparse matrix.</returns>
        public static SparseMatrix RandomSymmetric(int size, double density, bool definite)
        {
            return RandomSymmetric(size, density, definite, new Random(RANDOM_SEED));
        }

        /// <summary>
        /// Create a random symmetric sparse matrix.
        /// </summary>
        /// <param name="size">The size of the matrix.</param>
        /// <param name="density">The density (between 0.0 and 1.0).</param>
        /// <param name="definite">If true, the matrix will be positive semi-definite.</param>
        /// <param name="random">The random source.</param>
        /// <returns>Random sparse matrix.</returns>
        public static SparseMatrix RandomSymmetric(int size, double density, bool definite, Random random)
        {
            if (size < 0)
            {
                throw new ArgumentException(Resources.ValueNonNegative, nameof(size));
            }

            // Number of non-zeros per row.
            int nz = (int)Math.Max(size * size * density, 1d);

            var C = new CoordinateStorage<double>(size, size, nz);

            int m = (nz - size) / 2;

            var norm = new double[size];

            for (int k = 0; k < m; k++)
            {
                int i = (int)Math.Min(random.NextDouble() * size, size - 1);
                int j = (int)Math.Min(random.NextDouble() * size, size - 1);

                if (i != j)
                {
                    var value = random.NextDouble();

                    norm[i] += Math.Abs(value);
                    norm[j] += Math.Abs(value);

                    C.At(i, j, value);
                    C.At(j, i, value);
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

                C.At(i, i, value);
            }

            return SparseMatrix.OfIndexed(C) as SparseMatrix;
        }

        /// <summary>
        /// Get the 1D Laplacian matrix (with Dirichlet boundary conditions).
        /// </summary>
        /// <param name="nx">Grid size.</param>
        /// <param name="eigenvalues">Vector to store eigenvalues (optional).</param>
        /// <returns>Laplacian sparse matrix.</returns>
        public static SparseMatrix Laplacian(int nx, double[] eigenvalues = null)
        {
            if (nx == 1)
            {
                // Handle special case n = 1.
                var A = new SparseMatrix(nx, nx, 1);

                A.Values[0] = 2.0;

                return A;
            }

            var C = new CoordinateStorage<double>(nx, nx, nx);

            for (int i = 0; i < nx; i++)
            {
                C.At(i, i, 2.0);

                if (i == 0)
                {
                    C.At(i, i + 1, -1.0);
                }
                else if (i == (nx - 1))
                {
                    C.At(i, i - 1, -1.0);
                }
                else
                {
                    C.At(i, i - 1, -1.0);
                    C.At(i, i + 1, -1.0);
                }
            }

            if (eigenvalues != null)
            {
                // Compute eigenvalues.
                int count = Math.Min(nx, eigenvalues.Length);

                var eigs = new double[nx];

                for (int i = 0; i < count; i++)
                {
                    eigs[i] = 4 * Math.Pow(Math.Sin((i + 1) * Math.PI / (2 * (nx + 1))), 2);
                }

                Array.Sort(eigs);

                for (int i = 0; i < count; ++i)
                {
                    eigenvalues[i] = eigs[i];
                }
            }

            return SparseMatrix.OfIndexed(C) as SparseMatrix;
        }

        /// <summary>
        /// Get the 2D Laplacian matrix (with Dirichlet boundary conditions).
        /// </summary>
        /// <param name="nx">Number of elements in x direction.</param>
        /// <param name="ny">Number of elements in y direction.</param>
        /// <param name="eigenvalues">Vector to store eigenvalues (optional).</param>
        /// <returns>Laplacian sparse matrix.</returns>
        public static SparseMatrix Laplacian(int nx, int ny, double[] eigenvalues = null)
        {
            var Ix = Eye(nx);
            var Iy = Eye(ny);

            var Dx = Laplacian(nx);
            var Dy = Laplacian(ny);

            if (eigenvalues != null)
            {
                // Compute eigenvalues.
                int count = Math.Min(nx * ny, eigenvalues.Length);
                int index = 0;

                var eigs = new double[nx * ny];

                double ax, ay;

                for (int i = 0; i < nx; i++)
                {
                    ax = 4 * Math.Pow(Math.Sin((i + 1) * Math.PI / (2 * (nx + 1))), 2);
                    for (int j = 0; j < ny; j++)
                    {
                        ay = 4 * Math.Pow(Math.Sin((j + 1) * Math.PI / (2 * (ny + 1))), 2);
                        eigs[index++] = ax + ay;
                    }
                }

                Array.Sort(eigs);

                for (int i = 0; i < count; ++i)
                {
                    eigenvalues[i] = eigs[i];
                }
            }

            return (SparseMatrix)(Kron(Iy, Dx).Add(Kron(Dy, Ix)));
        }

        // Wathen elements.
        private static readonly double[] em =
        {
             6.0, -6.0,  2.0, -8.0,  3.0, -8.0,  2.0, -6.0, 
            -6.0, 32.0, -6.0, 20.0, -8.0, 16.0, -8.0, 20.0, 
             2.0, -6.0,  6.0, -6.0,  2.0, -8.0,  3.0, -8.0, 
            -8.0, 20.0, -6.0, 32.0, -6.0, 20.0, -8.0, 16.0, 
             3.0, -8.0,  2.0, -6.0,  6.0, -6.0,  2.0, -8.0, 
            -8.0, 16.0, -8.0, 20.0, -6.0, 32.0, -6.0, 20.0, 
             2.0, -8.0,  3.0, -8.0,  2.0, -6.0,  6.0, -6.0, 
            -6.0, 20.0, -8.0, 16.0, -8.0, 20.0, -6.0, 32.0
        };

        /// <summary>
        /// Create Wathen finite element matrix.
        /// </summary>
        /// <param name="nx">Number of elements in x direction.</param>
        /// <param name="ny">Number of elements in y direction.</param>
        /// <param name="seed">Random seed.</param>
        /// <returns>Wathen sparse matrix.</returns>
        /// <remarks>
        /// The entries of the matrix depend in part on a physical quantity
        /// related to density. That density is here assigned random values between
        /// 0 and 100.
        ///
        /// The matrix order N is determined by the input quantities NX and NY,
        /// which would usually be the number of elements in the X and Y directions.
        ///
        /// The value of N is
        ///
        ///   N = 3*NX*NY + 2*NX + 2*NY + 1,
        ///
        /// The matrix is the consistent mass matrix for a regular NX by NY grid
        /// of 8 node serendipity elements.
        ///
        /// The matrix is symmetric positive definite for any positive values of the
        /// density RHO(X,Y).
        /// </remarks>
        public static SparseMatrix Wathen(int nx, int ny, int seed = 834651)
        {
            var random = new Random(seed);

            int order = 3 * nx * ny + 2 * nx + 2 * ny + 1;

            var C = new CoordinateStorage<double>(order, order, nx * ny * 64);

            int kcol;
            int krow;
            int[] node = new int[8];
            double rho;

            for (int j = 1; j <= ny; j++)
            {
                for (int i = 1; i <= nx; i++)
                {
                    node[0] = 3 * j * nx + 2 * j + 2 * i;
                    node[1] = node[0] - 1;
                    node[2] = node[1] - 1;
                    node[3] = (3 * j - 1) * nx + 2 * j + i - 2;
                    node[4] = (3 * j - 3) * nx + 2 * j + 2 * i - 4;
                    node[5] = node[4] + 1;
                    node[6] = node[5] + 1;
                    node[7] = node[3] + 1;

                    rho = 100.0 * random.NextDouble();

                    for (krow = 0; krow < 8; krow++)
                    {
                        for (kcol = 0; kcol < 8; kcol++)
                        {
                            C.At(node[krow], node[kcol], rho * em[krow + kcol * 8]);
                        }
                    }
                }
            }

            return SparseMatrix.OfIndexed(C) as SparseMatrix;
        }

        private static SparseMatrix Kron(SparseMatrix A, SparseMatrix B)
        {
            return A.KroneckerProduct(B);
        }
    }
}
