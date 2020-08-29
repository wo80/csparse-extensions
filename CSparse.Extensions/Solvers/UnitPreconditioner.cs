// Inspired by Math.NET Numerics (MIT license)
// https://github.com/mathnet/mathnet-numerics

namespace CSparse.Solvers
{
    using System;
    using CSparse.Properties;

    /// <summary>
    /// Unit preconditioner that does not do anything.
    /// </summary>
    /// <remarks>
    /// This preconditioner is used when running an <see cref="IIterativeSolver{T}"/> without a given preconditioner.
    /// </remarks>
    public sealed class UnitPreconditioner<T> : IPreconditioner<T> where T : struct, IEquatable<T>, IFormattable
    {
        readonly int size;

        /// <summary>
        /// Initializes the preconditioner and loads the internal data structures.
        /// </summary>
        /// <param name="matrix">The matrix upon which the preconditioner is based.</param>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        public UnitPreconditioner(ILinearOperator<T> matrix)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.MatrixSquare, nameof(matrix));
            }

            size = matrix.RowCount;
        }

        /// <inheritdoc/>
        public void Apply(T[] b, T[] x)
        {
            Array.Copy(b, x, size);
        }
    }
}
