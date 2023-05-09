// Inspired by Math.NET Numerics (MIT license)
// https://github.com/mathnet/mathnet-numerics

namespace CSparse.Solvers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An iterator that is used to check if an iterative calculation should continue or stop.
    /// </summary>
    public sealed class Iterator<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// The collection that holds all the stop criteria and the flag indicating if they should be added
        /// to the child iterators.
        /// </summary>
        readonly List<IIterationStopCriterion<T>> _stopCriteria;

        /// <summary>
        /// The status of the iterator.
        /// </summary>
        IterationStatus _status = IterationStatus.Continue;

        /// <summary>
        /// The number of iterations taken.
        /// </summary>
        int _iterations = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Iterator{T}"/> class with the specified stop criteria.
        /// </summary>
        /// <param name="stopCriteria">
        /// The specified stop criteria. Only one stop criterion of each type can be passed in. None
        /// of the stop criteria will be passed on to child iterators.
        /// </param>
        public Iterator(params IIterationStopCriterion<T>[] stopCriteria)
        {
            if (!stopCriteria.Any())
            {
                throw new ArgumentException("No stop criterion specified.");
            }

            _stopCriteria = new List<IIterationStopCriterion<T>>(stopCriteria);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Iterator{T}"/> class with the specified stop criteria.
        /// </summary>
        /// <param name="stopCriteria">
        /// The specified stop criteria. Only one stop criterion of each type can be passed in. None
        /// of the stop criteria will be passed on to child iterators.
        /// </param>
        public Iterator(IEnumerable<IIterationStopCriterion<T>> stopCriteria)
        {
            if (!stopCriteria.Any())
            {
                throw new ArgumentException("No stop criterion specified.");
            }

            _stopCriteria = new List<IIterationStopCriterion<T>>(stopCriteria);
        }

        /// <summary>
        /// Gets the list of stop criteria.
        /// </summary>
        public IEnumerable<IIterationStopCriterion<T>> StopCriteria
        {
            get { return _stopCriteria; }
        }

        /// <summary>
        /// Gets or sets the current calculation status.
        /// </summary>
        public IterationStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        /// <summary>
        /// Determines the status of the iterative calculation based on the stop criteria stored
        /// by the current <see cref="Iterator{T}"/>. Result is set into <c>Status</c> field.
        /// </summary>
        /// <param name="iterationNumber">The number of iterations that have passed so far.</param>
        /// <param name="residualVectorNorm">The current residual vector norm.</param>
        /// <remarks>
        /// The individual iterators may internally track the progress of the calculation based
        /// on the invocation of this method. Therefore this method should only be called if the
        /// calculation has moved forwards at least one step.
        /// </remarks>
        public IterationStatus DetermineStatus(int iterationNumber, double residualVectorNorm)
        {
            if (iterationNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterationNumber));
            }

            _iterations = iterationNumber;

            // While we're canceled we don't call on the stop-criteria.
            if (_status == IterationStatus.Cancelled)
            {
                return _status;
            }

            foreach (var stopCriterion in _stopCriteria)
            {
                var status = stopCriterion.DetermineStatus(iterationNumber, residualVectorNorm);
                if (status == IterationStatus.Continue)
                {
                    continue;
                }

                _status = status;
                return _status;
            }

            // Got all the way through
            // So we're running because we had vectors passed to us.
            _status = IterationStatus.Continue;

            return _status;
        }

        /// <summary>
        /// Indicates to the iterator that the iterative process has been canceled.
        /// </summary>
        /// <remarks>
        /// Does not reset the stop-criteria.
        /// </remarks>
        public void Cancel()
        {
            _status = IterationStatus.Cancelled;
        }

        /// <summary>
        /// Resets the <see cref="Iterator{T}"/> to the pre-calculation state.
        /// </summary>
        public void Reset()
        {
            _status = IterationStatus.Continue;

            foreach (var stopCriterion in _stopCriteria)
            {
                stopCriterion.Reset();
            }
        }

        /// <summary>
        /// Creates a deep clone of the current iterator.
        /// </summary>
        /// <returns>The deep clone of the current iterator.</returns>
        public Iterator<T> Clone()
        {
            return new Iterator<T>(_stopCriteria.Select(sc => sc.Clone()));
        }
    }
}
