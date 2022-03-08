
namespace CSparse.Extensions.Tests.Double.Solvers
{
    using CSparse.Double;
    using CSparse.Double.Preconditioner;
    using CSparse.Double.Solvers;
    using CSparse.Solvers;
    using NUnit.Framework;
    using System.Collections.Generic;

    public class MinResTest
    {
        [Test]
        public void TestSolve()
        {
            const int N = 100;

            var A = CreateSparse.RandomSymmetric(N, 0.1, true);
            var x = Vector.Create(N, 1.0);
            var b = new double[N];

            A.Multiply(x, b);

            var iterator = new Iterator<double>(new List<IIterationStopCriterion<double>>()
            {
                new IterationCountStopCriterion<double>(N),
                new ResidualStopCriterion<double>(1e-8)
            });

            Vector.Clear(x);

            var solver = new MinRes();

            solver.Solve(A, b, x, iterator, new DiagonalPreconditioner(A));

            Assert.AreEqual(iterator.Status, IterationStatus.Converged);
        }
    }
}
