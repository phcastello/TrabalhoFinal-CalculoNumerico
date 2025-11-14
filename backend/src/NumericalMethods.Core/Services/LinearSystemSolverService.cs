using System;
using System.Diagnostics;
using NumericalMethods.Core.Common;
using NumericalMethods.Core.LinearSystems;

namespace NumericalMethods.Core.Services;

public sealed class LinearSystemSolverService : ILinearSystemSolverService
{
    private const double PivotTolerance = 1e-12;
    private const double SymmetryTolerance = 1e-10;
    private const double DivergenceThreshold = 1e12;

    public SolverResult Solve(
        LinearSystem system,
        LinearSolverMethod method,
        IterativeParams? iterativeParams = null,
        bool returnSteps = false)
    {
        return method switch
        {
            LinearSolverMethod.Gauss => SolveWithGauss(system),
            LinearSolverMethod.GaussPartialPivoting => SolveWithGaussPartialPivoting(system),
            LinearSolverMethod.GaussFullPivoting => SolveWithGaussFullPivoting(system),
            LinearSolverMethod.LU => SolveWithLu(system),
            LinearSolverMethod.Cholesky => SolveWithCholesky(system),
            LinearSolverMethod.Jacobi => SolveIteratively(system, iterativeParams, SolveWithJacobiIteration),
            LinearSolverMethod.GaussSeidel => SolveIteratively(system, iterativeParams, SolveWithGaussSeidelIteration),
            _ => new SolverResult
            {
                Status = SolverStatus.NotImplemented,
                Message = "Método não reconhecido para solução de sistemas lineares."
            }
        };
    }

    private SolverResult SolveWithGauss(LinearSystem system)
    {
        var stopwatch = Stopwatch.StartNew();
        var n = system.N;
        var a = LinearAlgebraUtils.CloneMatrix(system.A);
        var b = LinearAlgebraUtils.CloneVector(system.B);

        for (var k = 0; k < n; k++)
        {
            var pivot = a[k, k];
            if (Math.Abs(pivot) < PivotTolerance)
            {
                return SingularResult("Pivô próximo de zero durante eliminação de Gauss sem pivoteamento.", stopwatch);
            }

            for (var i = k + 1; i < n; i++)
            {
                var factor = a[i, k] / pivot;
                for (var j = k; j < n; j++)
                {
                    a[i, j] -= factor * a[k, j];
                }

                b[i] -= factor * b[k];
            }
        }

        try
        {
            var solution = LinearAlgebraUtils.BackwardSubstitution(a, b);
            stopwatch.Stop();
            return SuccessResult(solution, 0, stopwatch);
        }
        catch (InvalidOperationException ex)
        {
            return SingularResult(ex.Message, stopwatch);
        }
    }

    private SolverResult SolveWithGaussPartialPivoting(LinearSystem system)
    {
        var stopwatch = Stopwatch.StartNew();
        var n = system.N;
        var a = LinearAlgebraUtils.CloneMatrix(system.A);
        var b = LinearAlgebraUtils.CloneVector(system.B);

        for (var k = 0; k < n; k++)
        {
            var pivotRow = k;
            var max = Math.Abs(a[k, k]);
            for (var i = k + 1; i < n; i++)
            {
                var value = Math.Abs(a[i, k]);
                if (value > max)
                {
                    max = value;
                    pivotRow = i;
                }
            }

            if (max < PivotTolerance)
            {
                return SingularResult("Pivô próximo de zero durante Gauss com pivoteamento parcial.", stopwatch);
            }

            if (pivotRow != k)
            {
                SwapRows(a, k, pivotRow);
                (b[k], b[pivotRow]) = (b[pivotRow], b[k]);
            }

            var pivot = a[k, k];
            for (var i = k + 1; i < n; i++)
            {
                var factor = a[i, k] / pivot;
                for (var j = k; j < n; j++)
                {
                    a[i, j] -= factor * a[k, j];
                }

                b[i] -= factor * b[k];
            }
        }

        try
        {
            var solution = LinearAlgebraUtils.BackwardSubstitution(a, b);
            stopwatch.Stop();
            return SuccessResult(solution, 0, stopwatch);
        }
        catch (InvalidOperationException ex)
        {
            return SingularResult(ex.Message, stopwatch);
        }
    }

    private SolverResult SolveWithGaussFullPivoting(LinearSystem system)
    {
        var stopwatch = Stopwatch.StartNew();
        var n = system.N;
        var a = LinearAlgebraUtils.CloneMatrix(system.A);
        var b = LinearAlgebraUtils.CloneVector(system.B);
        var perm = new int[n];
        for (var i = 0; i < n; i++)
        {
            perm[i] = i;
        }

        for (var k = 0; k < n; k++)
        {
            var pivotRow = k;
            var pivotCol = k;
            var max = 0.0;

            for (var i = k; i < n; i++)
            {
                for (var j = k; j < n; j++)
                {
                    var value = Math.Abs(a[i, j]);
                    if (value > max)
                    {
                        max = value;
                        pivotRow = i;
                        pivotCol = j;
                    }
                }
            }

            if (max < PivotTolerance)
            {
                return SingularResult("Pivô próximo de zero durante Gauss com pivoteamento completo.", stopwatch);
            }

            if (pivotRow != k)
            {
                SwapRows(a, k, pivotRow);
                (b[k], b[pivotRow]) = (b[pivotRow], b[k]);
            }

            if (pivotCol != k)
            {
                SwapColumns(a, k, pivotCol);
                (perm[k], perm[pivotCol]) = (perm[pivotCol], perm[k]);
            }

            var pivot = a[k, k];
            for (var i = k + 1; i < n; i++)
            {
                var factor = a[i, k] / pivot;
                for (var j = k; j < n; j++)
                {
                    a[i, j] -= factor * a[k, j];
                }

                b[i] -= factor * b[k];
            }
        }

        try
        {
            var xPermuted = LinearAlgebraUtils.BackwardSubstitution(a, b);
            var solution = new double[n];
            for (var i = 0; i < n; i++)
            {
                solution[perm[i]] = xPermuted[i];
            }

            stopwatch.Stop();
            return SuccessResult(solution, 0, stopwatch);
        }
        catch (InvalidOperationException ex)
        {
            return SingularResult(ex.Message, stopwatch);
        }
    }

    private SolverResult SolveWithLu(LinearSystem system)
    {
        var stopwatch = Stopwatch.StartNew();
        var n = system.N;
        var A = LinearAlgebraUtils.CloneMatrix(system.A);
        var b = LinearAlgebraUtils.CloneVector(system.B);
        var L = new double[n, n];
        var U = new double[n, n];

        for (var i = 0; i < n; i++)
        {
            for (var k = i; k < n; k++)
            {
                double sum = 0;
                for (var j = 0; j < i; j++)
                {
                    sum += L[i, j] * U[j, k];
                }

                U[i, k] = A[i, k] - sum;
            }

            for (var k = i; k < n; k++)
            {
                if (i == k)
                {
                    L[i, i] = 1;
                }
                else
                {
                    double sum = 0;
                    for (var j = 0; j < i; j++)
                    {
                        sum += L[k, j] * U[j, i];
                    }

                    var pivot = U[i, i];
                    if (Math.Abs(pivot) < PivotTolerance)
                    {
                        return SingularResult("Pivô próximo de zero durante fatoração LU.", stopwatch);
                    }

                    L[k, i] = (A[k, i] - sum) / pivot;
                }
            }
        }

        try
        {
            var y = LinearAlgebraUtils.ForwardSubstitution(L, b);
            var solution = LinearAlgebraUtils.BackwardSubstitution(U, y);
            stopwatch.Stop();
            return SuccessResult(solution, 0, stopwatch);
        }
        catch (InvalidOperationException ex)
        {
            return SingularResult(ex.Message, stopwatch);
        }
    }

    private SolverResult SolveWithCholesky(LinearSystem system)
    {
        var stopwatch = Stopwatch.StartNew();
        var n = system.N;
        var A = LinearAlgebraUtils.CloneMatrix(system.A);
        var b = LinearAlgebraUtils.CloneVector(system.B);

        if (!IsSymmetric(A))
        {
            stopwatch.Stop();
            return new SolverResult
            {
                Status = SolverStatus.NotSPD,
                Message = "Matriz não é simétrica; Cholesky não aplicável.",
                Solution = Array.Empty<double>(),
                Iterations = 0,
                ElapsedMs = stopwatch.Elapsed.TotalMilliseconds
            };
        }

        var L = new double[n, n];

        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j <= i; j++)
            {
                double sum = 0;
                for (var k = 0; k < j; k++)
                {
                    sum += L[i, k] * L[j, k];
                }

                if (i == j)
                {
                    var value = A[i, i] - sum;
                    if (value <= SymmetryTolerance)
                    {
                        stopwatch.Stop();
                        return new SolverResult
                        {
                            Status = SolverStatus.NotSPD,
                            Message = "Matriz não é definida positiva; Cholesky não aplicável.",
                            Solution = Array.Empty<double>(),
                            Iterations = 0,
                            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds
                        };
                    }

                    L[i, i] = Math.Sqrt(value);
                }
                else
                {
                    var pivot = L[j, j];
                    if (LinearAlgebraUtils.IsApproximatelyZero(pivot))
                    {
                        stopwatch.Stop();
                        return new SolverResult
                        {
                            Status = SolverStatus.NotSPD,
                            Message = "Matriz não é definida positiva; Cholesky não aplicável.",
                            Solution = Array.Empty<double>(),
                            Iterations = 0,
                            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds
                        };
                    }

                    L[i, j] = (A[i, j] - sum) / pivot;
                }
            }
        }

        try
        {
            var y = LinearAlgebraUtils.ForwardSubstitution(L, b);
            var solution = BackwardSubstitutionWithTranspose(L, y);
            stopwatch.Stop();
            return SuccessResult(solution, 0, stopwatch);
        }
        catch (InvalidOperationException ex)
        {
            return SingularResult(ex.Message, stopwatch);
        }
    }

    private SolverResult SolveIteratively(
        LinearSystem system,
        IterativeParams? iterativeParams,
        Func<double[,], double[], IterativeParams, SolverResult> solver)
    {
        if (iterativeParams is null)
        {
            return new SolverResult
            {
                Status = SolverStatus.InvalidInput,
                Message = "Parâmetros iterativos são obrigatórios para o método escolhido."
            };
        }

        if (iterativeParams.Tolerance <= 0)
        {
            return new SolverResult
            {
                Status = SolverStatus.InvalidInput,
                Message = "Tolerância deve ser maior que zero."
            };
        }

        if (iterativeParams.MaxIterations <= 0)
        {
            return new SolverResult
            {
                Status = SolverStatus.InvalidInput,
                Message = "Número máximo de iterações deve ser maior que zero."
            };
        }

        var A = LinearAlgebraUtils.CloneMatrix(system.A);
        var b = LinearAlgebraUtils.CloneVector(system.B);
        return solver(A, b, iterativeParams);
    }

    private SolverResult SolveWithJacobiIteration(double[,] A, double[] b, IterativeParams parameters)
    {
        var stopwatch = Stopwatch.StartNew();
        var n = b.Length;
        var xOld = new double[n];
        var xNew = new double[n];

        for (var i = 0; i < n; i++)
        {
            if (LinearAlgebraUtils.IsApproximatelyZero(A[i, i]))
            {
                stopwatch.Stop();
                return new SolverResult
                {
                    Status = SolverStatus.InvalidInput,
                    Message = "Diagonal contém zero; Jacobi não pode ser aplicado.",
                    Iterations = 0,
                    ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                    Solution = Array.Empty<double>()
                };
            }
        }

        for (var iteration = 1; iteration <= parameters.MaxIterations; iteration++)
        {
            for (var i = 0; i < n; i++)
            {
                double sum = 0;
                for (var j = 0; j < n; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }

                    sum += A[i, j] * xOld[j];
                }

                xNew[i] = (b[i] - sum) / A[i, i];
            }

            var stopValue = ComputeStopValue(A, b, xOld, xNew, parameters.StopCondition);
            if (double.IsNaN(stopValue) || double.IsInfinity(stopValue) || VectorNormGreaterThan(xNew, DivergenceThreshold))
            {
                stopwatch.Stop();
                return new SolverResult
                {
                    Status = SolverStatus.Divergence,
                    Message = "Iterações divergiram no método de Jacobi.",
                    Iterations = iteration,
                    ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                    Solution = LinearAlgebraUtils.CloneVector(xNew)
                };
            }

            if (stopValue < parameters.Tolerance)
            {
                stopwatch.Stop();
                return SuccessResult(LinearAlgebraUtils.CloneVector(xNew), iteration, stopwatch);
            }

            Array.Copy(xNew, xOld, n);
        }

        stopwatch.Stop();
        return new SolverResult
        {
            Status = SolverStatus.MaxIterationsReached,
            Iterations = parameters.MaxIterations,
            Solution = LinearAlgebraUtils.CloneVector(xNew),
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
            Message = "Número máximo de iterações atingido no método de Jacobi."
        };
    }

    private SolverResult SolveWithGaussSeidelIteration(double[,] A, double[] b, IterativeParams parameters)
    {
        var stopwatch = Stopwatch.StartNew();
        var n = b.Length;
        var x = new double[n];

        for (var i = 0; i < n; i++)
        {
            if (LinearAlgebraUtils.IsApproximatelyZero(A[i, i]))
            {
                stopwatch.Stop();
                return new SolverResult
                {
                    Status = SolverStatus.InvalidInput,
                    Message = "Diagonal contém zero; Gauss-Seidel não pode ser aplicado.",
                    Iterations = 0,
                    ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                    Solution = Array.Empty<double>()
                };
            }
        }

        for (var iteration = 1; iteration <= parameters.MaxIterations; iteration++)
        {
            var previous = LinearAlgebraUtils.CloneVector(x);

            for (var i = 0; i < n; i++)
            {
                double sum = 0;
                for (var j = 0; j < n; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }

                    sum += A[i, j] * (j < i ? x[j] : previous[j]);
                }

                x[i] = (b[i] - sum) / A[i, i];
            }

            var stopValue = ComputeStopValue(A, b, previous, x, parameters.StopCondition);
            if (double.IsNaN(stopValue) || double.IsInfinity(stopValue) || VectorNormGreaterThan(x, DivergenceThreshold))
            {
                stopwatch.Stop();
                return new SolverResult
                {
                    Status = SolverStatus.Divergence,
                    Message = "Iterações divergiram no método de Gauss-Seidel.",
                    Iterations = iteration,
                    ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                    Solution = LinearAlgebraUtils.CloneVector(x)
                };
            }

            if (stopValue < parameters.Tolerance)
            {
                stopwatch.Stop();
                return SuccessResult(LinearAlgebraUtils.CloneVector(x), iteration, stopwatch);
            }
        }

        stopwatch.Stop();
        return new SolverResult
        {
            Status = SolverStatus.MaxIterationsReached,
            Iterations = parameters.MaxIterations,
            Solution = LinearAlgebraUtils.CloneVector(x),
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
            Message = "Número máximo de iterações atingido no método de Gauss-Seidel."
        };
    }

    private static double ComputeStopValue(
        double[,] A,
        double[] b,
        double[] previous,
        double[] current,
        IterativeStopCondition condition)
    {
        return condition switch
        {
            IterativeStopCondition.ByResidual =>
                LinearAlgebraUtils.Norm2(SubtractVectors(LinearAlgebraUtils.Multiply(A, current), b)),
            IterativeStopCondition.ByDeltaX =>
                LinearAlgebraUtils.Norm2(SubtractVectors(current, previous)),
            _ => double.PositiveInfinity
        };
    }

    private static double[] SubtractVectors(double[] left, double[] right)
    {
        var result = new double[left.Length];
        for (var i = 0; i < left.Length; i++)
        {
            result[i] = left[i] - right[i];
        }

        return result;
    }

    private static bool VectorNormGreaterThan(double[] vector, double threshold)
    {
        foreach (var value in vector)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return true;
            }
        }

        return LinearAlgebraUtils.Norm2(vector) > threshold;
    }

    private static void SwapRows(double[,] matrix, int rowA, int rowB)
    {
        if (rowA == rowB)
        {
            return;
        }

        var cols = matrix.GetLength(1);
        for (var j = 0; j < cols; j++)
        {
            (matrix[rowA, j], matrix[rowB, j]) = (matrix[rowB, j], matrix[rowA, j]);
        }
    }

    private static void SwapColumns(double[,] matrix, int colA, int colB)
    {
        if (colA == colB)
        {
            return;
        }

        var rows = matrix.GetLength(0);
        for (var i = 0; i < rows; i++)
        {
            (matrix[i, colA], matrix[i, colB]) = (matrix[i, colB], matrix[i, colA]);
        }
    }

    private static bool IsSymmetric(double[,] matrix)
    {
        var n = matrix.GetLength(0);
        for (var i = 0; i < n; i++)
        {
            for (var j = i + 1; j < n; j++)
            {
                if (Math.Abs(matrix[i, j] - matrix[j, i]) > SymmetryTolerance)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static double[] BackwardSubstitutionWithTranspose(double[,] L, double[] y)
    {
        var n = y.Length;
        var x = new double[n];

        for (var i = n - 1; i >= 0; i--)
        {
            double sum = 0;
            for (var j = i + 1; j < n; j++)
            {
                sum += L[j, i] * x[j];
            }

            var pivot = L[i, i];
            if (LinearAlgebraUtils.IsApproximatelyZero(pivot))
            {
                throw new InvalidOperationException("Pivot zero ao resolver L^T x = y.");
            }

            x[i] = (y[i] - sum) / pivot;
        }

        return x;
    }

    private static SolverResult SuccessResult(double[] solution, int iterations, Stopwatch stopwatch)
    {
        return new SolverResult
        {
            Status = SolverStatus.Success,
            Solution = solution,
            Iterations = iterations,
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
            Message = string.Empty
        };
    }

    private static SolverResult SingularResult(string message, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        return new SolverResult
        {
            Status = SolverStatus.SingularMatrix,
            Message = message,
            Solution = Array.Empty<double>(),
            Iterations = 0,
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds
        };
    }
}
