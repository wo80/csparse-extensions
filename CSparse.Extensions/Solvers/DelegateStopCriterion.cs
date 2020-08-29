// Inspired by Math.NET Numerics (MIT license)
// https://github.com/mathnet/mathnet-numerics

namespace CSparse.Solvers
{
    using System;

    /// <summary>
    /// Stop criterion that delegates the status determination to a delegate.
    /// </summary>
    public class DelegateStopCriterion<T> : IIterationStopCriterion<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        readonly Func<int, double, IterationStatus> _determine;
        IterationStatus _status = IterationStatus.Continue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateStopCriterion{T}"/> class.
        /// </summary>
        /// <param name="determine">Custom implementation with the same signature and semantics as the DetermineStatus method.</param>
        public DelegateStopCriterion(Func<int, double, IterationStatus> determine)
        {
            _determine = determine;
        }

        /// <inheritdoc/>
        public IterationStatus DetermineStatus(int iterationNumber, double residualVectorNorm)
        {
            return _status = _determine(iterationNumber, residualVectorNorm);
        }

        /// <inheritdoc/>
        public IterationStatus Status
        {
            get { return _status; }
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _status = IterationStatus.Continue;
        }

        /// <inheritdoc/>
        public IIterationStopCriterion<T> Clone()
        {
            return new DelegateStopCriterion<T>(_determine);
        }
    }
}
