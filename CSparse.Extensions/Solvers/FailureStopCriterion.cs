// Inspired by Math.NET Numerics (MIT license)
// https://github.com/mathnet/mathnet-numerics

namespace CSparse.Solvers
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Defines an <see cref="IIterationStopCriterion{T}"/> that monitors residuals for NaN's.
    /// </summary>
    public sealed class FailureStopCriterion<T> : IIterationStopCriterion<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// The status of the calculation
        /// </summary>
        IterationStatus _status = IterationStatus.Continue;

        /// <summary>
        /// The iteration number of the last iteration.
        /// </summary>
        int _lastIteration = -1;

        /// <inheritdoc/>
        public IterationStatus DetermineStatus(int iterationNumber, double residualVectorNorm)
        {
            if (_lastIteration >= iterationNumber)
            {
                // We have already stored the actual last iteration number
                // For now do nothing. We only care about the next step.
                return _status;
            }

            // Store the infinity norms of both the solution and residual vectors
            double residualNorm = residualVectorNorm;

            _status = double.IsNaN(residualNorm) ? IterationStatus.Failure : IterationStatus.Continue;

            _lastIteration = iterationNumber;
            return _status;
        }

        /// <inheritdoc/>
        public IterationStatus Status
        {
            [DebuggerStepThrough]
            get { return _status; }
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _status = IterationStatus.Continue;
            _lastIteration = -1;
        }

        /// <inheritdoc/>
        public IIterationStopCriterion<T> Clone()
        {
            return new FailureStopCriterion<T>();
        }
    }
}
