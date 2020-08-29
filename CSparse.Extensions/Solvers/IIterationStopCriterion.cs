// Inspired by Math.NET Numerics (MIT license)
// https://github.com/mathnet/mathnet-numerics

namespace CSparse.Solvers
{
    using System;

    /// <summary>
    /// The base interface for classes that provide stop criteria for iterative calculations.
    /// </summary>
    public interface IIterationStopCriterion<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Determines the status of the iterative calculation based on the stop criteria stored
        /// by the current <see cref="IIterationStopCriterion{T}"/>. Status is set to <c>Status</c>
        /// field of current object.
        /// </summary>
        /// <param name="iterationNumber">The number of iterations that have passed so far.</param>
        /// <param name="residualVectorNorm">The norm of the current residual vector.</param>
        /// <remarks>
        /// The individual stop criteria may internally track the progress of the calculation based
        /// on the invocation of this method. Therefore this method should only be called if the
        /// calculation has moved forwards at least one step.
        /// </remarks>
        IterationStatus DetermineStatus(int iterationNumber, double residualVectorNorm);

        /// <summary>
        /// Gets or sets the current calculation status.
        /// </summary>
        IterationStatus Status { get; }

        /// <summary>
        /// Resets the IIterationStopCriterion to the pre-calculation state.
        /// </summary>
        /// <remarks>
        /// To implementers: Invoking this method should not clear the user defined property
        /// values, only the state that is used to track the progress of the calculation.
        /// </remarks>
        void Reset();

        /// <summary>
        /// Returns a clone of the <see cref="IIterationStopCriterion{T}"/> instance.
        /// </summary>
        IIterationStopCriterion<T> Clone();
    }
}
