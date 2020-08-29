// Inspired by Math.NET Numerics (MIT license)
// https://github.com/mathnet/mathnet-numerics

namespace CSparse.Solvers
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Monitors an iterative calculation for signs of divergence.
    /// </summary>
    public sealed class DivergenceStopCriterion<T> : IIterationStopCriterion<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// The maximum relative increase the residual may experience without triggering a divergence warning.
        /// </summary>
        double _maximumRelativeIncrease;

        /// <summary>
        /// The number of iterations over which a residual increase should be tracked before issuing a divergence warning.
        /// </summary>
        int _minimumNumberOfIterations;

        /// <summary>
        /// The status of the calculation
        /// </summary>
        IterationStatus _status = IterationStatus.Continue;

        /// <summary>
        /// The array that holds the tracking information.
        /// </summary>
        double[] _residualHistory;

        /// <summary>
        /// The iteration number of the last iteration.
        /// </summary>
        int _lastIteration = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="DivergenceStopCriterion{T}"/> class.
        /// </summary>
        /// <param name="maximumRelativeIncrease">The maximum relative increase that the residual may experience before a divergence warning is issued. </param>
        /// <param name="minimumIterations">The minimum number of iterations over which the residual must grow before a divergence warning is issued.</param>
        public DivergenceStopCriterion(double maximumRelativeIncrease = 0.08, int minimumIterations = 10)
        {
            if (maximumRelativeIncrease <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumRelativeIncrease));
            }

            // There must be at least three iterations otherwise we can't calculate the relative increase
            if (minimumIterations < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumIterations));
            }

            _maximumRelativeIncrease = maximumRelativeIncrease;
            _minimumNumberOfIterations = minimumIterations;
        }

        /// <summary>
        /// Gets or sets the maximum relative increase that the residual may experience before a divergence warning is issued.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <c>Maximum</c> is set to zero or below.</exception>
        public double MaximumRelativeIncrease
        {
            [DebuggerStepThrough]
            get { return _maximumRelativeIncrease; }

            [DebuggerStepThrough]
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _maximumRelativeIncrease = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum number of iterations over which the residual must grow before
        /// issuing a divergence warning.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <c>value</c> is set to less than one.</exception>
        public int MinimumNumberOfIterations
        {
            [DebuggerStepThrough]
            get { return _minimumNumberOfIterations; }

            [DebuggerStepThrough]
            set
            {
                // There must be at least three iterations otherwise we can't calculate
                // the relative increase
                if (value < 3)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _minimumNumberOfIterations = value;
            }
        }

        /// <inheritdoc/>
        public IterationStatus DetermineStatus(int iterationNumber, double residualVectorNorm)
        {
            if (_lastIteration >= iterationNumber)
            {
                // We have already stored the actual last iteration number
                // For now do nothing. We only care about the next step.
                return _status;
            }

            if ((_residualHistory == null) || (_residualHistory.Length != RequiredHistoryLength))
            {
                _residualHistory = new double[RequiredHistoryLength];
            }

            // We always track the residual.
            // Move the old versions one element up in the array.
            for (var i = 1; i < _residualHistory.Length; i++)
            {
                _residualHistory[i - 1] = _residualHistory[i];
            }

            // Store the infinity norms of both the solution and residual vectors
            // These values will be used to calculate the relative drop in residuals later on.
            _residualHistory[_residualHistory.Length - 1] = residualVectorNorm;

            // Check if we have NaN's. If so we've gone way beyond normal divergence.
            // Stop the iteration.
            if (double.IsNaN(_residualHistory[_residualHistory.Length - 1]))
            {
                _status = IterationStatus.Diverged;
                return _status;
            }

            // Check if we are diverging and if so set the status
            _status = IsDiverging() ? IterationStatus.Diverged : IterationStatus.Continue;

            _lastIteration = iterationNumber;
            return _status;
        }

        /// <summary>
        /// Detect if solution is diverging
        /// </summary>
        /// <returns><c>true</c> if diverging, otherwise <c>false</c></returns>
        bool IsDiverging()
        {
            // Run for each variable
            for (var i = 1; i < _residualHistory.Length; i++)
            {
                var difference = _residualHistory[i] - _residualHistory[i - 1];

                // Divergence is occurring if:
                // - the last residual is larger than the previous one
                // - the relative increase of the residual is larger than the setting allows
                if ((difference < 0) || (_residualHistory[i - 1]*(1 + _maximumRelativeIncrease) >= _residualHistory[i]))
                {
                    // No divergence taking place within the required number of iterations
                    // So reset and stop the iteration. There is no way we can get to the
                    // required number of iterations anymore.
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets required history Length
        /// </summary>
        int RequiredHistoryLength
        {
            [DebuggerStepThrough]
            get { return _minimumNumberOfIterations + 1; }
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
            _residualHistory = null;
        }

        /// <inheritdoc/>
        public IIterationStopCriterion<T> Clone()
        {
            return new DivergenceStopCriterion<T>(_maximumRelativeIncrease, _minimumNumberOfIterations);
        }
    }
}
