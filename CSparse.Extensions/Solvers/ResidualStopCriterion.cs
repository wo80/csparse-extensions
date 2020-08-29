// Inspired by Math.NET Numerics (MIT license)
// https://github.com/mathnet/mathnet-numerics

namespace CSparse.Solvers
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Defines an <see cref="IIterationStopCriterion{T}"/> that monitors residuals as stop criterion.
    /// </summary>
    public sealed class ResidualStopCriterion<T> : IIterationStopCriterion<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// The status of the calculation
        /// </summary>
        IterationStatus _status = IterationStatus.Continue;

        /// <summary>
        /// The maximum value for the residual below which the calculation is considered converged.
        /// </summary>
        double _tolerance;

        /// <summary>
        /// The minimum number of iterations for which the residual has to be below the maximum before
        /// the calculation is considered converged.
        /// </summary>
        int _minimumIterationsBelowMaximum;

        /// <summary>
        /// The number of iterations since the residuals got below the maximum.
        /// </summary>
        int _iterationCount;

        /// <summary>
        /// The iteration number of the last iteration.
        /// </summary>
        int _lastIteration = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResidualStopCriterion{T}"/> class.
        /// </summary>
        /// <param name="tolerance">
        /// The tolerance for the residual below which the calculation is considered converged.
        /// </param>
        /// <param name="minimumIterationsBelowMaximum">
        /// The minimum number of iterations for which the residual has to be below the maximum before
        /// the calculation is considered converged.
        /// </param>
        public ResidualStopCriterion(double tolerance, int minimumIterationsBelowMaximum = 0)
        {
            if (tolerance < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerance));
            }

            if (minimumIterationsBelowMaximum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumIterationsBelowMaximum));
            }

            _tolerance = tolerance;
            _minimumIterationsBelowMaximum = minimumIterationsBelowMaximum;
        }

        /// <summary>
        /// Gets or sets the tolerance for the residual below which the calculation is considered
        /// converged.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <c>Tolerance</c> is set to a negative value.</exception>
        public double Tolerance
        {
            [DebuggerStepThrough]
            get { return _tolerance; }

            [DebuggerStepThrough]
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _tolerance = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum number of iterations for which the residual has to be
        /// below the maximum before the calculation is considered converged.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <c>MinimumIterationsBelowMaximum</c> is set to a value less than 0.
        /// </exception>
        public int MinimumIterationsBelowMaximum
        {
            [DebuggerStepThrough]
            get { return _minimumIterationsBelowMaximum; }

            [DebuggerStepThrough]
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _minimumIterationsBelowMaximum = value;
            }
        }

        /// <inheritdoc/>
        public IterationStatus DetermineStatus(int iterationNumber, double residualVectorNorm)
        {
            // Store the infinity norms of both the solution and residual vectors
            // These values will be used to calculate the relative drop in residuals
            // later on.
            var residualNorm = residualVectorNorm;

            // First check that we have real numbers not NaN's.
            // NaN's can occur when the iterative process diverges so we
            // stop if that is the case.
            if (double.IsNaN(residualNorm))
            {
                _iterationCount = 0;
                _status = IterationStatus.Diverged;
                return _status;
            }

            // ||r_i|| <= stop_tol * ||b||
            // Stop the calculation if it's clearly smaller than the tolerance
            if (residualNorm <= _tolerance)
            {
                if (_lastIteration <= iterationNumber)
                {
                    _iterationCount = iterationNumber - _lastIteration;
                    _status = _iterationCount >= _minimumIterationsBelowMaximum ? IterationStatus.Converged : IterationStatus.Continue;
                }
            }
            else
            {
                _iterationCount = 0;
                _status = IterationStatus.Continue;
            }

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
            _iterationCount = 0;
            _lastIteration = -1;
        }

        /// <inheritdoc/>
        public IIterationStopCriterion<T> Clone()
        {
            return new ResidualStopCriterion<T>(_tolerance, _minimumIterationsBelowMaximum);
        }
    }
}
