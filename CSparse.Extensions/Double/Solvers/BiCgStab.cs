
namespace CSparse.Double.Solvers
{
    using CSparse.Solvers;
    using System;

    /// <summary>
    /// BiCGStab algorithm.
    /// </summary>
    /// <remarks>
    /// Based on Hypre code: https://github.com/hypre-space/hypre
    /// SPDX-License-Identifier: (Apache-2.0 OR MIT)
    /// </remarks>
    public class BiCgStab : IIterativeSolver<double>
    {
        private const double TINY = 1.0e-128;

        double atol;
        double rel_residual_norm;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiCgStab"/> class.
        /// </summary>
        public BiCgStab()
            : this(0.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiCgStab"/> class.
        /// </summary>
        /// <param name="atol">Absolute tolerance (if 0.0, relative tolerance of <see cref="ResidualStopCriterion{T}"/> is used).</param>
        public BiCgStab(double atol)
        {
            this.atol = atol;
        }

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix, b is the
        /// solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="matrix">The coefficient <see cref="ILinearOperator{T}"/>, <c>A</c>.</param>
        /// <param name="input">The solution <see cref="Vector"/>, <c>b</c>.</param>
        /// <param name="result">The result <see cref="Vector"/>, <c>x</c>.</param>
        /// <param name="iterator">The iterator to use to control when to stop iterating.</param>
        /// <param name="preconditioner">The preconditioner to use for approximations.</param>
        public void Solve(ILinearOperator<double> matrix, double[] input, double[] result,
            Iterator<double> iterator, IPreconditioner<double> preconditioner)
        {
            var A = matrix;
            var M = preconditioner ?? new UnitPreconditioner<double>(matrix);

            var b = input;
            var x = result;

            int min_iter = 0;

            int n = b.Length;

            double[] r = new double[n];
            double[] r0 = new double[n];
            double[] s = new double[n];
            double[] v = new double[n];
            double[] p = new double[n];
            double[] q = new double[n];

            double alpha, beta, gamma, temp, res;
            double epsilon, r_norm, b_norm;
            //double cf_ave_0 = 0.0;
            //double cf_ave_1 = 0.0;
            //double weight;
            double r_norm_0;
            double den_norm;
            double gamma_numer;
            double gamma_denom;

            // Initialize work arrays
            Vector.Copy(b, r0);

            // Compute initial residual r0 = b - Ax
            A.Multiply(-1.0, x, 1.0, r0);
            Vector.Copy(r0, r);
            Vector.Copy(r0, p);

            b_norm = Vector.Norm(n, b);

            res = Vector.DotProduct(n, r0, r0);
            r_norm = Math.Sqrt(res);
            r_norm_0 = r_norm;

            int i = 0;

            if (b_norm > 0.0)
            {
                // convergence criterion |r_i| <= r_tol*|b| if |b| > 0
                den_norm = b_norm;
            }
            else
            {
                // convergence criterion |r_i| <= r_tol*|r0| if |b| = 0
                den_norm = r_norm;
            }

            // convergence criteria: |r_i| <= max( a_tol, r_tol * den_norm)
            // den_norm = |r_0| or |b|
            // note: default for a_tol is 0.0, so relative residual criteria is used unless
            //       user also specifies a_tol or sets r_tol = 0.0, which means absolute
            //       tol only is checked

            double rtol = iterator.GetTolerance();

            epsilon = Math.Max(atol, rtol * den_norm);

            if (b_norm > 0.0)
            {
                rel_residual_norm = r_norm / b_norm;
            }

            // Start BiCGStab iterations
            while (iterator.DetermineStatus(i, r_norm) == IterationStatus.Continue)
            {
                i++;

                M.Apply(p, v);
                A.Multiply(v, q);
                temp = Vector.DotProduct(n, r0, q);
                if (Math.Abs(temp) < TINY)
                {
                    iterator.Status = IterationStatus.Failure; // TODO: numerical breakdown
                    return;
                }
                alpha = res / temp;
                Vector.Axpy(alpha, v, x);
                Vector.Axpy(-alpha, q, r);
                M.Apply(r, v);

                A.Multiply(v, s);
                // Handle case when gamma = 0.0/0.0 as 0.0 and not NAN */
                gamma_numer = Vector.DotProduct(n, r, s);
                gamma_denom = Vector.DotProduct(n, s, s);
                if ((gamma_numer == 0.0) && (gamma_denom == 0.0))
                {
                    gamma = 0.0;
                }
                else
                {
                    gamma = gamma_numer / gamma_denom;
                }

                Vector.Axpy(gamma, v, x);
                Vector.Axpy(-gamma, s, r);
                // residual is now updated, must check for convergence
                r_norm = Vector.Norm(n, r);

                // check for convergence, evaluate actual residual
                if (r_norm <= epsilon && i >= min_iter)
                {
                    Vector.Copy(b, r);
                    A.Multiply(-1.0, x, 1.0, r);
                    r_norm = Vector.Norm(n, r);
                    if (r_norm <= epsilon)
                    {
                        iterator.Status = IterationStatus.Converged;
                        break;
                    }
                }

                if (Math.Abs(res) < TINY)
                {
                    iterator.Status = IterationStatus.Failure; // TODO: numerical breakdown
                    return;
                }

                beta = 1.0 / res;
                res = Vector.DotProduct(n, r0, r);
                beta *= res;
                Vector.Axpy(-gamma, q, p);

                if (Math.Abs(gamma) < TINY)
                {
                    iterator.Status = IterationStatus.Failure; // TODO: numerical breakdown
                    return;
                }

                Vector.Scale(beta * alpha / gamma, p);
                Vector.Axpy(1.0, r, p);
            }

            rel_residual_norm = b_norm == 0.0 ? r_norm : r_norm / b_norm;
        }
    }
}
