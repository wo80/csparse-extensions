namespace CSparse
{
    using CSparse.Properties;
    using CSparse.Storage;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// CompressedColumnStorage extension methods.
    /// </summary>
    public static class CompressedColumnStorageExtensions
    {
        /// <summary>
        /// Count all matrix entries that match the given predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix">The matrix.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static int Count<T>(this CompressedColumnStorage<T> matrix, Func<int, int, T, bool> predicate)
            where T : struct, IEquatable<T>, IFormattable
        {
            int columns = matrix.ColumnCount;

            var ax = matrix.Values;
            var ap = matrix.ColumnPointers;
            var ai = matrix.RowIndices;

            int count = 0;

            for (int i = 0; i < columns; i++)
            {
                int end = ap[i + 1];
                for (int j = ap[i]; j < end; j++)
                {
                    if (predicate(ai[j], i, ax[j]))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Test if any matrix entry satisfies the given predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix">The matrix.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static bool Any<T>(this CompressedColumnStorage<T> matrix, Func<int, int, T, bool> predicate)
            where T : struct, IEquatable<T>, IFormattable
        {
            int columns = matrix.ColumnCount;

            var ax = matrix.Values;
            var ap = matrix.ColumnPointers;
            var ai = matrix.RowIndices;

            for (int i = 0; i < columns; i++)
            {
                int end = ap[i + 1];
                for (int j = ap[i]; j < end; j++)
                {
                    if (predicate(ai[j], i, ax[j]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Enumerate all matrix entries that match the given predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix">The matrix.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static IEnumerable<Tuple<int, int, T>> EnumerateIndexed<T>(this CompressedColumnStorage<T> matrix, Func<int, int, T, bool> predicate)
            where T : struct, IEquatable<T>, IFormattable
        {
            int columns = matrix.ColumnCount;

            var ax = matrix.Values;
            var ap = matrix.ColumnPointers;
            var ai = matrix.RowIndices;

            for (int i = 0; i < columns; i++)
            {
                int end = ap[i + 1];
                for (int j = ap[i]; j < end; j++)
                {
                    if (predicate(ai[j], i, ax[j]))
                    {
                        yield return new Tuple<int, int, T>(ai[j], i, ax[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Extract a sub matrix from a symmetric matrix with given row and column indices.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix">The matrix.</param>
        /// <param name="indices">The indices of the rows and columns to extract.</param>
        /// <returns>The sub matrix.</returns>
        /// <remarks>
        /// The indices have to be in order. The method also work, if only the lower part of the input matrix is passed.
        /// </remarks>
        public static CompressedColumnStorage<T> SubMatrix<T>(this CompressedColumnStorage<T> matrix, int[] indices)
            where T : struct, IEquatable<T>, IFormattable
        {
            int n = matrix.ColumnCount;

            if (n != matrix.RowCount)
            {
                throw new Exception(Resources.MatrixSquare);
            }

            int size = indices.Length;

            var mask = new HashSet<int>(indices);

            int nnz = matrix.Count((i, j, _) => i > j && mask.Contains(i) && mask.Contains(j));

            var result = Create<T>(size, size, 2 * nnz + size);

            SubMatrix(matrix, indices, result);

            return result;
        }

        /// <summary>
        /// Extract a sub matrix from a symmetric matrix with given row and column indices.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix">The matrix.</param>
        /// <param name="indices">The indices of the rows and columns to extract.</param>
        /// <param name="target">The target sub matrix (has to provide enough memory for the non-zeros).</param>
        /// <remarks>
        /// The indices have to be in order. The method also work, if only the lower part of the input matrix is passed.
        /// </remarks>
        public static void SubMatrix<T>(this CompressedColumnStorage<T> matrix, int[] indices, CompressedColumnStorage<T> target)
            where T : struct, IEquatable<T>, IFormattable
        {
            int n = matrix.ColumnCount;

            if (n != matrix.RowCount)
            {
                throw new ArgumentException(Resources.MatrixSquare, nameof(matrix));
            }

            var ax = matrix.Values;
            var ap = matrix.ColumnPointers;
            var ai = matrix.RowIndices;

            int size = indices.Length;

            if (target.RowCount != size || target.ColumnCount != size)
            {
                throw new ArgumentException(Resources.InvalidDimensions, nameof(target));
            }

            // Mask of rows/columns to extract.
            var mask = new bool[n];

            for (int i = 0, j, last = -1; i < size; i++)
            {
                j = indices[i];

                if (j <= last)
                {
                    throw new ArgumentException("Indices have to be sorted.", nameof(indices));
                }

                last = j;

                mask[j] = true;
            }

            // Maps old row indices to new ones.
            var map = new int[n];

            for (int i = 0, j = 0; i < n; i++)
            {
                map[i] = j;

                if (mask[i]) j++;
            }

            var bx = target.Values;
            var bp = target.ColumnPointers;
            var bi = target.RowIndices;

            int k = 0;

            for (int i = 0; i < size; i++)
            {
                bp[i] = k;

                // Current column to copy.
                int column = indices[i];

                var end = ap[column + 1];

                for (var j = ap[column]; j < end; j++)
                {
                    int row = ai[j];

                    if (mask[row])
                    {
                        bi[k] = map[row];
                        bx[k] = ax[j];

                        k++;
                    }
                }
            }

            bp[size] = k;
        }

        /// <summary>
        /// Extract a sub matrix with given set of rows and columns.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix">The matrix.</param>
        /// <param name="rows">The rows to extract (passing <c>null</c> will extract all rows).</param>
        /// <param name="columns">The columns to extract (passing <c>null</c> will extract all columns).</param>
        /// <returns></returns>
        public static CompressedColumnStorage<T> SubMatrix<T>(this CompressedColumnStorage<T> matrix, int[] rows, int[] columns)
            where T : struct, IEquatable<T>, IFormattable
        {
            int rowCount = matrix.RowCount;
            int columnCount = matrix.ColumnCount;

            int rsize = rows?.Length ?? rowCount;
            int csize = columns?.Length ?? columnCount;

            var density = matrix.NonZerosCount / ((double)rowCount * columnCount);

            var c = new CoordinateStorage<T>(rsize, csize, (int)((rsize * csize) * density));

            var mask_r = new bool[rowCount];
            var map_r = new int[rowCount];

            var mask_c = new bool[columnCount];
            var map_c = new int[columnCount];

            int k = 0;

            for (int i = 0; i < rsize; i++)
            {
                int idx = rows == null ? i : rows[i];

                mask_r[idx] = true;
                map_r[idx] = k++;
            }

            k = 0;

            for (int i = 0; i < csize; i++)
            {
                int idx = columns == null ? i : columns[i];

                mask_c[idx] = true;
                map_c[idx] = k++;
            }

            foreach (var e in matrix.EnumerateIndexed())
            {
                int i = e.Item1;
                int j = e.Item2;

                if (mask_r[i] && mask_c[j])
                {
                    c.At(map_r[i], map_c[j], e.Item3);
                }
            }

            return CompressedColumnStorage<T>.OfIndexed(c, true);
        }

        /// <summary>
        /// Eliminate equations from a linear system by setting rows and columns of the
        /// symmetric matrix to identity (all values zero except diagonal one).
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="indices">The indices of the rows and columns to eliminate.</param>
        public static void EliminateSymmetric<T>(this CompressedColumnStorage<T> matrix, int[] indices)
            where T : struct, IEquatable<T>, IFormattable
        {
            T ZERO = Helper.ZeroOf<T>();
            T ONE = Helper.OneOf<T>();

            int n = matrix.ColumnCount;

            if (n != matrix.RowCount)
            {
                throw new Exception(Resources.MatrixSquare);
            }

            var mask = new bool[n];

            for (int i = 0; i < indices.Length; i++)
            {
                mask[indices[i]] = true;
            }

            var ax = matrix.Values;
            var ap = matrix.ColumnPointers;
            var ai = matrix.RowIndices;

            for (int i = 0; i < n; i++)
            {
                var end = ap[i + 1];

                if (mask[i])
                {
                    // Eliminate column.
                    for (var j = ap[i]; j < end; j++)
                    {
                        ax[j] = ai[j] == i ? ONE : ZERO;
                    }
                }
                else
                {
                    for (var j = ap[i]; j < end; j++)
                    {
                        int row = ai[j];

                        // Eliminate row index.
                        if (mask[row])
                        {
                            ax[j] = row == i ? ONE : ZERO;
                        }
                    }
                }
            }

            matrix.DropZeros();
        }

        internal static CompressedColumnStorage<T> Create<T>(int rowCount, int columnCount, int valueCount)
            where T : struct, IEquatable<T>, IFormattable
        {
            if (typeof(T) == typeof(double))
            {
                return new CSparse.Double.SparseMatrix(rowCount, columnCount, valueCount)
                    as CompressedColumnStorage<T>;
            }

            if (typeof(T) == typeof(System.Numerics.Complex))
            {
                return new CSparse.Complex.SparseMatrix(rowCount, columnCount, valueCount)
                    as CompressedColumnStorage<T>;
            }

            throw new NotSupportedException();
        }
    }
}
