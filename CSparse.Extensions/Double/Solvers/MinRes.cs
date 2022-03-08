// Based on MATLAB MINRES files by Michael Saunders, SOL and ICME, Stanford University (16 Jun 2020)
// https://web.stanford.edu/group/SOL/software/minres/

namespace CSparse.Double.Solvers
{
    using CSparse.Solvers;
    using System;

    /// <summary>
    /// MINRES  is designed to solve the system of linear equations <c>Ax = b</c> or the
    /// least-squares problem <c>min || Ax - b ||_2</c>, where A is an n-by-n symmetric
    /// matrix and b is a given vector. The matrix A may be indefinite and/or singular.
    /// </summary>
    /// <remarks>
    /// INPUT:
    ///
    /// x0     is an estimate of x, or [] if an estimate is not available.
    ///        minres computes r0 = b - A*x0, solves A dx = r0, and updates x := x0 + dx.
    ///        This guards against loss of low-order digits of dx if x0 is a good approximation
    ///        (r0 and dx are small).
    ///
    /// A      may be a dense or sparse matrix (preferably sparse!)
    ///
    /// b      is the right-hand side n-vector.
    ///
    /// M      is an optional preconditioner (or []).  Ideally, M should be SPD.
    ///        Depending on precon, M may be a dense or sparse matrix that approximates
    ///        A or inv(A) (really A - shift*I or inv(A - shift*I)),
    ///        or it may be a function handle such that y = M(x) solves My = x
    ///        or computes y = M*x.
    ///
    /// precon = 1 implies M = CC' ~= A and minres implicitly solves the system
    ///                               PAP'xbar = Pb,
    ///        i.e.                  Abar xbar = bbar,
    ///        where                         P = inv(C),
    ///                                   Abar = PAP',
    ///                                   bbar = Pb,
    ///        and returns the solution      x = P'xbar.
    ///        The associated residual is rbar = bbar - Abar xbar
    ///                                        = P(b - A*x).
    ///        Note that minres asks for systems M*x = y to be solved via y = M\x.
    ///        This will be efficient if you set M = decomposition(M1,'chol'); say,
    ///        meaning that you know M1 ~= A is SPD and chol(M1) is practical.
    ///
    ///        If A2 ~= A is SPD and it is feasible to factorize [L,D,P,S] = LDL(A2,thresh)
    ///        (say), it is possible to set M = decomposition(A2,'ldl'); and input M.
    ///        It will use the default options for ldl(A2) such as thresh = 0.01.
    ///        (We don't know how to change those.)
    ///        If A2 is indefinite, we would like to change D (with 1x1 and 2x2 blocks)
    ///        to positive definite D2 = |D| (using abs(eigenvalues of D)),
    ///        but we don't know how to replace D by D2 in the output of "decomposition".
    ///
    /// precon = 2 implies M = P'P ~= inv(A) and (again) minres implicitly solves the system
    ///                               PAP'xbar = Pb,
    ///        i.e.                  Abar xbar = bbar,
    ///        where                      Abar = PAP',
    ///                                   bbar = Pb.
    ///        and returns the solution      x = P'xbar.
    ///        The associated residual is rbar = bbar - Abar xbar
    ///                                        = P(b - Ax).
    ///        Note that minres asks for products y = M*x.  This is efficient if M is a
    ///        sparse matrix.
    ///
    /// shift  If shift ~= 0, minres treats "A" as (A - shift*I).
    ///        If shift is an eigenvalue of A, x should approximate an eigenvector of A.
    ///
    ///
    /// OUTPUT:
    /// betacheck indicates whether M seems to be indefinite.  It is normalized
    ///        and may be treated as an absolute number.
    ///        If precon==1, betacheck = y'inv(M)y / y'y for some y.
    ///        If precon==2, betacheck =      y'My / y'y for some y. 
    ///        If itn==0, y = b.
    ///        Output istop==10 means betacheck &lt; ptol.
    ///        With ptol in [1e-6,1e-2] (say), this indicates that
    ///        M is not safely positive definite.  Setting
    ///           gamma = facMPD*(ptol - betacheck);   M = M + gamma*speye(n);
    ///        with facMPD in [10,100] (say) will make M more SPD.
    /// </remarks>
    public sealed class MinRes : IIterativeSolver<double>
    {
        // A tolerance on certain inner products that should be positive if M is SPD.
        //    Set ptol in [1e-6,1e-2] (say).  See output betacheck.
        double ptol = 1e-6;

        //  An int giving the reason for termination...
        //
        //   -1        beta2 = 0 in the Lanczos iteration; i.e. the
        //             second Lanczos vector is zero.  This means the
        //             rhs is very special.
        //             If there is no preconditioner, b is an
        //             eigenvector of A.
        //             Otherwise (if precon is true), let My = b.
        //             If shift is zero, y is a solution of the
        //             generalized eigenvalue problem Ay = lambda My,
        //             with lambda = alpha1 from the Lanczos vectors.
        //
        //             In general, (A - shift*I)x = b
        //             has the solution         x = (1/alpha1) y
        //             where My = b.
        //
        //    0        b = 0, so the exact solution is x = 0.
        //             No iterations were performed.
        //
        //    1        A solution to Ax = b was found, given rtol
        //
        //    2        A least-squares solution was found, given rtol
        //
        //    3        Reasonable accuracy achieved, given eps
        //
        //    4        x has converged to an eigenvector
        //
        //    5        Acond has exceeded 0.1/eps
        //
        //    6        The matrix defined by Aprod does not appear
        //             to be symmetric.
        //             For certain vectors y = Av and r = Ay, the
        //             products y'y and r'v differ significantly.
        //
        //    7        The matrix defined by Msolve does not appear
        //             to be symmetric.
        //             For vectors satisfying My = v and Mr = y, the
        //             products y'y and r'v differ significantly.
        //
        //    8        An inner product of the form  x' M**(-1) x
        //             was not positive, so the preconditioning matrix
        //             M does not appear to be positive definite.
        //
        //    9        M  is not sufficiently SPD
        //
        //   10        rtol reduced max times on preconditioned system
        //
        //             If istop >= 5, the final x may not be an
        //             acceptable solution.
        int istop; // TODO: public status code

        // An estimate of the norm of the matrix operator
        //    Abar = P (A - shift*I) P', where P = C**(-1).
        double anorm;
        // An estimate of the condition of Abar above. This will usually be a
        // substantial under-estimate of the true condition.
        double acond;
        // An estimate of the norm of the final transformed residual vector,
        //    P (b  -  (A - shift*I) x).
        double rnorm;
        // An estimate of the norm of xbar. This is sqrt( x'Mx ). If precon is false,
        // ynorm is an estimate of norm(x).
        double ynorm;
        // Estimates norm(Ar_{k-1}) or norm(Abar rbar_{k-1}) if M exists.
        // NOTE THAT arnorm LAGS AN ITERATION BEHIND rnorm.
        double arnorm;
        // xnorm  is the final norm(x).
        double xnorm;

        /// <inheritdoc/>
        public void Solve(ILinearOperator<double> matrix, double[] input, double[] result,
            Iterator<double> iterator, IPreconditioner<double> preconditioner)
        {
            var A = matrix;
            var M = preconditioner;
            
            var x = result;
            var b = input;

            double rtol = iterator.GetTolerance(); 

            double z, cs, sn, phi, rhs1, rhs2, alfa,
                dbar, beta, gbar, oldb;

            double gmin, gmax, epsx;
            double beta1, gamma;
            double delta, denom;
            double epsln;

            double tnorm2, phibar, oldeps;
            double test1, test2;

            bool precon = (M != null);
            
            int n = matrix.RowCount;

            const double eps = Constants.MachineEpsilon;

            var x0 = Vector.Clone(x);
            var r0 = Vector.Clone(b);

            var bnorm = Vector.Norm(n, b);
            var x0norm = Vector.Norm(n, x0);
            var r0norm = bnorm;

            // If an initial guess x0 is given, update residual r0.
            if (x0norm > 0.0)
            {
                A.Multiply(-1.0, x0, 1.0, r0); // r0 = b - A * x0
                r0norm = Vector.Norm(n, r0);
            }

            istop = 0;

            double betacheck, dxnorm;

            // Set up y and v for the first Lanczos vector v1.
            // y  =  beta1 P' v1,  where  P = C**(-1).
            // v is really P' v1.
            
            var y = Vector.Clone(r0);

            if (precon)
            {
                M.Apply(r0, y);
            }

            beta1 = Vector.DotProduct(n, r0, y);
            betacheck = beta1 / Vector.DotProduct(n, r0, r0);

            if (beta1 == 0)
            {
                // If r0 = 0 exactly, stop with x = x0.
                iterator.Status = IterationStatus.Converged;
                istop = 0;
                return;
            }
            else if (precon && betacheck < ptol)
            {
                // Preconditioner M does not appear to be positive definite.
                iterator.Status = IterationStatus.Failure;
                istop = 9;
                return;
            }

            Vector.Clear(x);

            var r1 = Vector.Clone(r0);
            var r2 = Vector.Clone(r0);
            var v = Vector.Create(n, 0.0);
            var w = Vector.Create(n, 0.0);
            var w1 = Vector.Create(n, 0.0);
            var w2 = Vector.Create(n, 0.0);
            var xt = Vector.Create(n, 0.0);

            beta1 = Math.Sqrt(beta1); // Normalize y to get v1 later.

            // Initialize other quantities.
            oldb = 0.0;
            beta = beta1;
            dbar = 0.0;
            epsln = 0.0;
            phibar = beta1;
            rhs1 = beta1;
            rhs2 = 0.0;
            tnorm2 = 0.0;

            cs = -1.0;
            sn = 0.0;
            gmax = 0.0;
            gmin = double.MaxValue;

            // Counts initial rtol and later reduced values if any.
            int numrtol = 1;

            // Save original rtol.
            double rtol0 = rtol;

            test1 = r0norm;

            // Main iteration loop.
            int itn = 0;
            while (iterator.DetermineStatus(itn, test1) == IterationStatus.Continue)
            {
                itn++;

                // Normalize previous vector (in y).
                // v = vk if P = I
                Vector.Scale(n, 1.0 / beta, y, v);

                // y = A * v;
                A.Multiply(v, y);

                if (itn > 1)
                {
                    // y = y - (beta / oldb) * r1;
                    Vector.Add(n, -beta / oldb, r1, y, y);
                }

                alfa = Vector.DotProduct(n, v, y); // alphak

                // y = (-alfa / beta) * r2 + y;
                Vector.Axpy(-alfa / beta, r2, y);
                Vector.Copy(r2, r1);
                Vector.Copy(y, r2);
                if (precon)
                {
                    M.Apply(r2, y);
                }
                oldb = beta; // oldb = betak
                beta = Vector.DotProduct(n, r2, y); // beta = betak+1^2

                betacheck = beta / Vector.DotProduct(n, r2, r2);

                if (precon && betacheck < ptol)
                {
                    // M is not sufficiently SPD
                    istop = 9;
                    iterator.Status = IterationStatus.Failure;
                    break;
                }

                beta = Math.Sqrt(beta);
                tnorm2 = tnorm2 + alfa * alfa + oldb * oldb + beta * beta;

                if (itn == 1)
                {
                    // Initialize a few things.
                    if (beta / beta1 <= 10.0 * eps)
                    {
                        // beta2 = 0 or ~ 0.
                        istop = -1; // Terminate later.
                    }
                }

                // Apply previous rotation Qk-1 to get
                //   [deltak epslnk+1] = [cs  sn][dbark    0   ]
                //   [gbar k dbar k+1]   [sn -cs][alfak betak+1].

                oldeps = epsln;
                delta  = cs * dbar + sn * alfa; // delta1 = 0         deltak
                gbar   = sn * dbar - cs * alfa; // gbar 1 = alfa1     gbar k
                epsln  =             sn * beta; // epsln2 = 0         epslnk+1
                dbar   =            -cs * beta; // dbar 2 = beta2     dbar k+1
                
                double root = Math.Sqrt(gbar * gbar + dbar * dbar); // norm([gbar dbar])
                arnorm = phibar * root; // ||Ar{k-1}||

                // Compute the next plane rotation Qk

                gamma = Math.Sqrt(gbar * gbar + beta * beta); // gammak
                gamma = Math.Max(gamma, eps);
                cs = gbar / gamma;        // ck
                sn = beta / gamma;        // sk
                phi = cs * phibar;        // phik
                phibar = sn * phibar;     // phibark+1

                // Update x.

                // w1 = w2;
                // w2 = w;
                // w = (v - oldeps * w1 - delta * w2) * denom;
                // x = x + phi * w;

                denom = 1.0 / gamma;
                for (int i = 0; i < n; i++)
                {
                    w1[i] = w2[i];
                    w2[i] = w[i];
                    w[i] = (v[i] - oldeps * w1[i] - delta * w2[i]) * denom;
                    x[i] += phi * w[i];
                }

                // Go round again.

                gmax = Math.Max(gmax, gamma);
                gmin = Math.Min(gmin, gamma);
                z = rhs1 / gamma;
                rhs1 = rhs2 - delta * z;
                rhs2 =       -epsln * z;

                // Estimate various norms (for preconditioned system).
                // Arnorm for previous iteration is estimated above.

                anorm = Math.Sqrt(tnorm2);
                dxnorm = Vector.Norm(n, x);   // Really norm(dx), excludes x0
                rnorm = phibar;

                // Estimate cond(A).
                // In this version we look at the diagonals of  R  in the
                // factorization of the lower Hessenberg matrix,  Q * H = R,
                // where H is the tridiagonal matrix from Lanczos with one
                // extra row, beta(k+1) e_k^T.

                acond = gmax / gmin;

                if (istop != 0) break;   // Exit while itn < itnlim

                // ---------------------------------------------------------------
                // Check stopping criteria.
                // ---------------------------------------------------------------

                epsx = (anorm * dxnorm + beta1) * eps;
                test1 = rnorm / (anorm * dxnorm + bnorm); // ||r||/(||A|| ||x|| + ||b||) for precond system
                //     test2  = root / anorm;                    // ||Ar{k-1}||/(||A|| ||r_{k-1}||)    "   "   " 
                test2 = arnorm / (anorm * (rnorm + eps));

                // These tests work if rtol < eps
                double t1 = 1 + test1;
                double t2 = 1 + test2;
                if (t2 <= 1) istop = 2;
                if (t1 <= 1) istop = 1;

                if (acond >= 0.1 / eps) istop = 5;
                if (epsx >= beta1) istop = 3;
                if (test2 <= rtol) istop = 2;
                if (test1 <= rtol) istop = 1;

                if (precon & istop > 0 & istop <= 5)
                {
                    // Preconditioned system thinks it satisfied rtol.
                    // See if Ax = b is satisfied to rtol0.

                    double rnormk;

                    if (x0norm == 0.0)
                    {
                        xnorm = dxnorm;

                        Vector.Copy(b, v);
                        A.Multiply(-1.0, x, 1.0, v); // v = b - A * x
                        rnormk = Vector.Norm(n, v);
                    }
                    else
                    {
                        Vector.Add(n, 1.0, x0, x, xt); // xt = x0 + x;
                        xnorm = Vector.Norm(n, xt);

                        Vector.Copy(b, v);
                        A.Multiply(-1.0, xt, 1.0, v); // v = b - A * xt
                        rnormk = Vector.Norm(n, v);
                    }

                    double epsr = (anorm * xnorm + bnorm) * rtol0;

                    if (rnormk <= epsr)
                    {
                        istop = 1;
                    }
                    else if (numrtol < 5 & rtol > eps)
                    {
                        numrtol = numrtol + 1;
                        rtol = rtol / 10; // Reduce rtol and continue iteration.
                        istop = 0;

                        // TODO: iterator.Pause(2);
                    }
                    else
                    {
                        istop = 10;
                    }
                }

                if (istop != 0)
                {
                    iterator.Status = istop < 5 ? IterationStatus.Converged : IterationStatus.Failure;
                    break;
                }
            }

            // Final status.

            if (x0norm > 0)
            {
                Vector.Axpy(1.0, x0, x);
            }
        }
    }
}
