// Inspired by Math.NET Numerics (MIT license)
// https://github.com/mathnet/mathnet-numerics

namespace CSparse.Solvers
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Defines an <see cref="IIterationStopCriterion{T}"/> that uses a cancellation token as stop criterion.
    /// </summary>
    public sealed class CancellationStopCriterion<T> : IIterationStopCriterion<T> where T : struct, IEquatable<T>, IFormattable
    {
        readonly CancellationToken _masterToken;
        CancellationTokenSource _currentTcs;

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellationStopCriterion{T}"/> class.
        /// </summary>
        public CancellationStopCriterion()
        {
            _masterToken = CancellationToken.None;
            _currentTcs = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellationStopCriterion{T}"/> class.
        /// </summary>
        /// <param name="masterToken">The cancellation token.</param>
        public CancellationStopCriterion(CancellationToken masterToken)
        {
            _masterToken = masterToken;
            _currentTcs = CancellationTokenSource.CreateLinkedTokenSource(masterToken);
        }

        /// <summary>
        /// Cancel the iterative solver.
        /// </summary>
        public void Cancel()
        {
            _currentTcs.Cancel();
        }

        /// <inheritdoc/>
        public IterationStatus DetermineStatus(int iterationNumber, double residualVectorNorm)
        {
            return _currentTcs.Token.IsCancellationRequested ? IterationStatus.Cancelled : IterationStatus.Continue;
        }

        /// <inheritdoc/>
        public IterationStatus Status
        {
            [DebuggerStepThrough]
            get { return _currentTcs.Token.IsCancellationRequested ? IterationStatus.Cancelled : IterationStatus.Continue; }
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _currentTcs = CancellationTokenSource.CreateLinkedTokenSource(_masterToken);
        }

        /// <inheritdoc/>
        public IIterationStopCriterion<T> Clone()
        {
            return new CancellationStopCriterion<T>(_masterToken);
        }
    }
}
