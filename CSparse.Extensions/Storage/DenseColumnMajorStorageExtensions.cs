
namespace CSparse.Storage
{
    using System;

    /// <summary>
    /// <see cref="DenseColumnMajorStorage{T}"/> extension methods.
    /// </summary>
    public static class DenseColumnMajorStorageExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        /// <param name="row"></param>
        /// <param name="target"></param>
        public static void Row<T>(this DenseColumnMajorStorage<T> matrix, int row, DenseVector<T> target)
            where T : struct, IEquatable<T>, IFormattable
        {
            matrix.Row(row, target.Values);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        /// <param name="row"></param>
        /// <param name="values"></param>
        public static void SetRow<T>(this DenseColumnMajorStorage<T> matrix, int row, DenseVector<T> values)
            where T : struct, IEquatable<T>, IFormattable
        {
            matrix.SetRow(row, values.Values);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        /// <param name="column"></param>
        /// <param name="target"></param>
        public static void Column<T>(this DenseColumnMajorStorage<T> matrix, int column, DenseVector<T> target)
            where T : struct, IEquatable<T>, IFormattable
        {
            matrix.Column(column, target.Values);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        /// <param name="column"></param>
        /// <param name="values"></param>
        public static void SetColumn<T>(this DenseColumnMajorStorage<T> matrix, int column, DenseVector<T> values)
            where T : struct, IEquatable<T>, IFormattable
        {
            matrix.SetColumn(column, values.Values);
        }
    }
}
