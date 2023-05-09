
namespace CSparse.Extensions.Tests.Double.Solvers
{
    using CSparse.Double;
    using CSparse.Double.Preconditioner;
    using CSparse.Double.Solvers;
    using CSparse.Solvers;
    using NUnit.Framework;
    using System.Collections.Generic;

    public class BiCgStabTest
    {
        [Test]
        public void TestSolveUnsymmetric()
        {
            const int N = 100;

            var A = CreateSparse.Random(N, N, 0.1);
            var x = Vector.Create(N, 1.0);
            var b = new double[N];

            A.Multiply(x, b);

            var iterator = new Iterator<double>(new List<IIterationStopCriterion<double>>()
            {
                new IterationCountStopCriterion<double>(N),
                new ResidualStopCriterion<double>(1e-8)
            });

            Vector.Clear(x);

            var solver = new BiCgStab();

            solver.Solve(A, b, x, iterator, new MILU0(A));

            Assert.AreEqual(IterationStatus.Converged, iterator.Status);
        }

        [Test]
        public void TestSolveSymmetric()
        {
            const int N = 100;

            var A = CreateSparse.RandomSymmetric(N, 0.1);
            var x = Vector.Create(N, 1.0);
            var b = new double[N];

            A.Multiply(x, b);

            var iterator = new Iterator<double>(new List<IIterationStopCriterion<double>>()
            {
                new IterationCountStopCriterion<double>(N),
                new ResidualStopCriterion<double>(1e-8)
            });

            Vector.Clear(x);

            var solver = new BiCgStab();

            solver.Solve(A, b, x, iterator, new MILU0(A, true));

            Assert.AreEqual(IterationStatus.Converged, iterator.Status);
        }
    }
}
