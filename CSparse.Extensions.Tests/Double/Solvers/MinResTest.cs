
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
        public void TestSolveSymmetricPositiveDefinite()
        {
            const int N = 100;

            var A = CreateSparse.Laplacian(N);
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

            solver.Solve(A, b, x, iterator, new Diagonal(A));

            Assert.AreEqual(iterator.Status, IterationStatus.Converged);
        }

        [Test]
        public void TestSolveSymmetricIndefinite()
        {
            const int N = 100;

            var A = CreateSparse.RandomSymmetric(N, 0.1);
            var x = Vector.Create(N, 1.0);
            var b = new double[N];

            A.Multiply(x, b);

            var iterator = new Iterator<double>(new List<IIterationStopCriterion<double>>()
            {
                new IterationCountStopCriterion<double>(2 * N),
                new ResidualStopCriterion<double>(1e-4)
            });

            Vector.Clear(x);

            var solver = new MinRes();

            solver.Solve(A, b, x, iterator, new Diagonal(A));

            Assert.AreEqual(iterator.Status, IterationStatus.Converged);
        }
    }
}
