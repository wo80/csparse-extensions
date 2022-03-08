
namespace CSparse.Solvers
{
    using System;

    static class IteratorExtensions
    {
        private const double DefaultTolerance = 1e-8;

        public static double GetTolerance<T>(this Iterator<T> iterator)
             where T : struct, IEquatable<T>, IFormattable
        {
            foreach (var item in iterator.StopCriteria)
            {
                if (item is ResidualStopCriterion<T> s)
                {
                    return s.Tolerance;
                }
            }

            return DefaultTolerance;
        }
    }
}
