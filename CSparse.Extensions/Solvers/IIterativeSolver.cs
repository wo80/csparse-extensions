// Inspired by Math.NET Numerics (MIT license)
// https://github.com/mathnet/mathnet-numerics

namespace CSparse.Solvers
{
    using System;

    /// <summary>
    /// Defines the interface for <see cref="IIterativeSolver{T}"/> classes that solve
    /// the linear equation Ax = b in an iterative manner.
    /// </summary>
    public interface IIterativeSolver<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Solves the linear equation Ax = b, where A is the coefficient matrix, b is the
        /// solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="matrix">The linear operator (usually represented as a matrix).</param>
        /// <param name="input">The solution vector <c>b</c></param>
        /// <param name="result">The result vector <c>x</c></param>
        /// <param name="iterator">The <see cref="Iterator{T}"/> used to control when to stop iterating.</param>
        /// <param name="preconditioner">The preconditioner to use for approximations.</param>
        void Solve(ILinearOperator<T> matrix, T[] input, T[] result, Iterator<T> iterator, IPreconditioner<T> preconditioner);
    }
}
