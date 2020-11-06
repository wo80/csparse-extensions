namespace CSparse
{
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
    }
}
