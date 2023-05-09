
namespace CSparse.Extensions.Tests.Complex.Solvers
{
    using CSparse.Complex;
    using CSparse.Complex.Preconditioner;
    using CSparse.Complex.Solvers;
    using CSparse.Solvers;
    using NUnit.Framework;
    using System.Collections.Generic;

    using Complex = System.Numerics.Complex;

    public class BiCgStabTest
    {
        [Test]
        public void TestSolveUnsymmetric()
        {
            const int N = 100;

            var A = CreateSparse.Random(N, N, 0.1);
            var x = Vector.Create(N, 1.0);
            var b = new Complex[N];

            A.Multiply(x, b);

            var iterator = new Iterator<Complex>(new List<IIterationStopCriterion<Complex>>()
            {
                new IterationCountStopCriterion<Complex>(N),
                new ResidualStopCriterion<Complex>(1e-8)
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

            var A = CreateSparse.RandomSymmetric(N, 0.1, true);
            var x = Vector.Create(N, 1.0);
            var b = new Complex[N];

            A.Multiply(x, b);

            var iterator = new Iterator<Complex>(new List<IIterationStopCriterion<Complex>>()
            {
                new IterationCountStopCriterion<Complex>(N),
                new ResidualStopCriterion<Complex>(1e-8)
            });

            Vector.Clear(x);

            var solver = new BiCgStab();

            solver.Solve(A, b, x, iterator, new MILU0(A, true));

            Assert.AreEqual(IterationStatus.Converged, iterator.Status);
        }
    }
}
