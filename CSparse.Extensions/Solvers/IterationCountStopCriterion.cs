// Inspired by Math.NET Numerics (MIT license)
// https://github.com/mathnet/mathnet-numerics

namespace CSparse.Solvers
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Defines an <see cref="IIterationStopCriterion{T}"/> that monitors the numbers of iteration 
    /// steps as stop criterion.
    /// </summary>
    public sealed class IterationCountStopCriterion<T> : IIterationStopCriterion<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// The default value for the maximum number of iterations the process is allowed
        /// to perform.
        /// </summary>
        public const int DefaultMaximumNumberOfIterations = 1000;

        /// <summary>
        /// The maximum number of iterations the calculation is allowed to perform.
        /// </summary>
        int _maximumNumberOfIterations;

        /// <summary>
        /// The status of the calculation
        /// </summary>
        IterationStatus _status = IterationStatus.Continue;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationCountStopCriterion{T}"/> class.
        /// </summary>
        public IterationCountStopCriterion() : this(DefaultMaximumNumberOfIterations)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationCountStopCriterion{T}"/> class with the specified maximum
        /// number of iterations.
        /// </summary>
        /// <param name="maximumNumberOfIterations">The maximum number of iterations the calculation is allowed to perform.</param>
        public IterationCountStopCriterion(int maximumNumberOfIterations)
        {
            if (maximumNumberOfIterations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumNumberOfIterations));
            }

            _maximumNumberOfIterations = maximumNumberOfIterations;
        }

        /// <summary>
        /// Gets or sets the maximum number of iterations the calculation is allowed to perform.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <c>Maximum</c> is set to a negative value.</exception>
        public int MaximumNumberOfIterations
        {
            [DebuggerStepThrough]
            get { return _maximumNumberOfIterations; }

            [DebuggerStepThrough]
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _maximumNumberOfIterations = value;
            }
        }

        /// <inheritdoc/>
        public IterationStatus DetermineStatus(int iterationNumber, double residualVectorNorm)
        {
            _status = iterationNumber >= _maximumNumberOfIterations ? IterationStatus.StoppedWithoutConvergence : IterationStatus.Continue;

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
        }

        /// <inheritdoc/>
        public IIterationStopCriterion<T> Clone()
        {
            return new IterationCountStopCriterion<T>(_maximumNumberOfIterations);
        }
    }
}
