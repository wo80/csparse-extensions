﻿namespace CSparse
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
        /// Extract a submatrix with given set of rows and columns.
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
    }
}
