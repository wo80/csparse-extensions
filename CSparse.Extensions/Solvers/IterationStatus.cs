// Inspired by Math.NET Numerics (MIT license)
// https://github.com/mathnet/mathnet-numerics

namespace CSparse.Solvers
{
    /// <summary>
    /// Iteration status.
    /// </summary>
    public enum IterationStatus
    {
        /// <summary>
        /// Continue iteration.
        /// </summary>
        Continue = 0,

        /// <summary>
        /// Iteration converged.
        /// </summary>
        Converged,

        /// <summary>
        /// Iteration diverged.
        /// </summary>
        Diverged,

        /// <summary>
        /// Iteration stopped without convergence.
        /// </summary>
        StoppedWithoutConvergence,

        /// <summary>
        /// Iteration cancelled.
        /// </summary>
        Cancelled,

        /// <summary>
        /// Solver failed.
        /// </summary>
        Failure
    }
}
