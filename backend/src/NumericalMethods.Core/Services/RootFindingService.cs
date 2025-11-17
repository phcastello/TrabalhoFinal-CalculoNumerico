using System;
using System.Diagnostics;
using NumericalMethods.Core.Common;
using NumericalMethods.Core.RootFinding;

namespace NumericalMethods.Core.Services;

public sealed class RootFindingService : IRootFindingService
{
    private const double DerivativeStepScale = 1e-6;
    private const double DivergenceThreshold = 1e12;
    private const double SmallNumber = 1e-12;

    public RootFindingResult Solve(RootFindingRequest request, bool returnSteps = false)
    {
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
        {
            return Invalid("Expressão da função não pode ser vazia.");
        }

        if (request.Tolerance <= 0)
        {
            return Invalid("Tolerância deve ser maior que zero.");
        }

        if (request.MaxIterations <= 0)
        {
            return Invalid("Número máximo de iterações deve ser maior que zero.");
        }

        Func<double, double> function;
        try
        {
            function = ExpressionEvaluator.Compile(request.FunctionExpression);
        }
        catch (ExpressionParseException)
        {
            return Invalid("Expressão inválida para f(x).");
        }
        catch (Exception)
        {
            return Invalid("Não foi possível interpretar a expressão fornecida.");
        }

        Func<double, double>? phi = null;
        Func<double, double>? derivative = null;

        if (request.Method == RootFindingMethod.FixedPoint)
        {
            if (string.IsNullOrWhiteSpace(request.PhiExpression))
            {
                return Invalid("É necessário fornecer φ(x) para o método do ponto fixo.");
            }

            try
            {
                phi = ExpressionEvaluator.Compile(request.PhiExpression);
            }
            catch (ExpressionParseException)
            {
                return Invalid("Expressão inválida para φ(x).");
            }
            catch (Exception)
            {
                return Invalid("Não foi possível interpretar φ(x) fornecida.");
            }
        }

        if (request.Method == RootFindingMethod.Newton && !string.IsNullOrWhiteSpace(request.DerivativeExpression))
        {
            try
            {
                derivative = ExpressionEvaluator.Compile(request.DerivativeExpression);
            }
            catch (ExpressionParseException)
            {
                return Invalid("Expressão inválida para f'(x).");
            }
            catch (Exception)
            {
                return Invalid("Não foi possível interpretar a expressão fornecida para f'(x).");
            }
        }

        return request.Method switch
        {
            RootFindingMethod.Bisection => SolveWithBisection(function, request),
            RootFindingMethod.FixedPoint => SolveWithFixedPoint(function, phi!, request),
            RootFindingMethod.Newton => SolveWithNewton(function, derivative, request),
            RootFindingMethod.RegulaFalsi => SolveWithRegulaFalsi(function, request),
            RootFindingMethod.Secant => SolveWithSecant(function, request),
            _ => new RootFindingResult
            {
                Status = SolverStatus.NotImplemented,
                Message = "Método de raiz não reconhecido.",
                Root = null,
                Iterations = 0,
                ElapsedMs = 0
            }
        };
    }

    private RootFindingResult SolveWithBisection(Func<double, double> f, RootFindingRequest request)
    {
        if (!request.A.HasValue || !request.B.HasValue)
        {
            return Invalid("Parâmetros 'a' e 'b' são obrigatórios para a Bisseção.");
        }

        var a = request.A.Value;
        var b = request.B.Value;
        if (a >= b)
        {
            return Invalid("É necessário que 'a' seja menor que 'b' para a Bisseção.");
        }

        var stopwatch = Stopwatch.StartNew();
        var fa = f(a);
        var fb = f(b);

        if (fa * fb > 0)
        {
            stopwatch.Stop();
            return Invalid("f(a) e f(b) devem ter sinais opostos para a Bisseção.");
        }

        double c = double.NaN;

        var iterationCount = 0;
        if (Math.Abs(b - a) < request.Tolerance)
        {
            return Success((a + b) / 2.0, iterationCount, stopwatch);
        }

        while (Math.Abs(b - a) > request.Tolerance && iterationCount < request.MaxIterations)
        {
            iterationCount++;

            var finicio = f(a);
            var meio = (a + b) / 2.0;
            var fmeio = f(meio);

            if (HasDiverged(meio, fmeio))
            {
                return Divergence($"Iterações divergiram no método da Bisseção.", meio, iterationCount, stopwatch);
            }

            if (finicio * fmeio < 0)
            {
                b = meio;
                fb = fmeio;
            }
            else
            {
                a = meio;
                fa = fmeio;
            }
        }

        c = (a + b) / 2.0;
        if (Math.Abs(b - a) > request.Tolerance)
        {
            return MaxIterations(c, iterationCount, stopwatch);
        }

        return Success(c, iterationCount, stopwatch);
    }

    private RootFindingResult SolveWithRegulaFalsi(Func<double, double> f, RootFindingRequest request)
    {
        if (!request.A.HasValue || !request.B.HasValue)
        {
            return Invalid("Parâmetros 'a' e 'b' são obrigatórios para Regula Falsi.");
        }

        var a = request.A.Value;
        var b = request.B.Value;
        if (a >= b)
        {
            return Invalid("É necessário que 'a' seja menor que 'b' para Regula Falsi.");
        }

        var stopwatch = Stopwatch.StartNew();
        var fa = f(a);
        var fb = f(b);

        if (fa * fb > 0)
        {
            stopwatch.Stop();
            return Invalid("f(a) e f(b) devem ter sinais opostos para Regula Falsi.");
        }

        var delta1 = request.Tolerance;
        var delta2 = request.Tolerance;

        if (Math.Abs(b - a) < delta1)
        {
            return Success((a + b) / 2.0, 0, stopwatch);
        }

        if (Math.Abs(fa) < delta2)
        {
            return Success(a, 0, stopwatch);
        }

        if (Math.Abs(fb) < delta2)
        {
            return Success(b, 0, stopwatch);
        }

        var iteration = 1;

        while (true)
        {
            var M = fa;
            var denominator = fb - fa;
            if (Math.Abs(denominator) < SmallNumber)
            {
                return Divergence("Denominador próximo de zero em Regula Falsi.", double.NaN, iteration, stopwatch);
            }

            var x = (a * fb - b * fa) / denominator;
            var fx = f(x);

            if (HasDiverged(x, fx))
            {
                return Divergence("Iterações divergiram em Regula Falsi.", x, iteration, stopwatch);
            }

            if (Math.Abs(fx) < delta2 || iteration > request.MaxIterations)
            {
                return iteration > request.MaxIterations
                    ? MaxIterations(x, iteration, stopwatch)
                    : Success(x, iteration, stopwatch);
            }

            if (M * fx > 0)
            {
                a = x;
                fa = fx;
            }
            else
            {
                b = x;
                fb = fx;
            }

            if (Math.Abs(b - a) < delta1)
            {
                var midpoint = (a + b) / 2.0;
                return Success(midpoint, iteration, stopwatch);
            }

            iteration++;
        }
    }

    private RootFindingResult SolveWithSecant(Func<double, double> f, RootFindingRequest request)
    {
        var firstGuess = request.InitialGuess;
        var secondGuess = request.SecondGuess;

        if (!firstGuess.HasValue || !secondGuess.HasValue)
        {
            if (request.A.HasValue && request.B.HasValue)
            {
                firstGuess ??= request.A.Value;
                secondGuess ??= request.B.Value;
            }
        }

        if (!firstGuess.HasValue || !secondGuess.HasValue)
        {
            return Invalid("É necessário fornecer dois chutes iniciais para a Secante.");
        }

        if (Math.Abs(firstGuess.Value - secondGuess.Value) < SmallNumber)
        {
            return Invalid("Os chutes iniciais para a Secante devem ser distintos.");
        }

        var x0 = firstGuess.Value;
        var x1 = secondGuess.Value;
        var f0 = f(x0);
        var f1 = f(x1);
        var stopwatch = Stopwatch.StartNew();

        if (Math.Abs(f0) < request.Tolerance)
        {
            return Success(x0, 0, stopwatch);
        }

        if (Math.Abs(f1) < request.Tolerance || Math.Abs(x1 - x0) < request.Tolerance)
        {
            return Success(x1, 0, stopwatch);
        }

        var iteration = 1;

        while (true)
        {
            var denominator = f1 - f0;
            if (Math.Abs(denominator) < SmallNumber)
            {
                return Divergence("Denominador próximo de zero no método da Secante.", x1, iteration, stopwatch);
            }

            var x2 = x1 - f1 * (x1 - x0) / denominator;
            var f2 = f(x2);

            if (HasDiverged(x2, f2))
            {
                return Divergence("Iterações divergiram no método da Secante.", x2, iteration, stopwatch);
            }

            if (Math.Abs(f2) < request.Tolerance || Math.Abs(x2 - x1) < request.Tolerance || iteration > request.MaxIterations)
            {
                return iteration > request.MaxIterations
                    ? MaxIterations(x2, iteration, stopwatch)
                    : Success(x2, iteration, stopwatch);
            }

            x0 = x1;
            f0 = f1;
            x1 = x2;
            f1 = f2;
            iteration++;
        }
    }

    private RootFindingResult SolveWithNewton(Func<double, double> f, Func<double, double>? derivative, RootFindingRequest request)
    {
        if (!request.InitialGuess.HasValue)
        {
            return Invalid("É necessário fornecer um chute inicial para o método de Newton.");
        }

        var x0 = request.InitialGuess.Value;
        var stopwatch = Stopwatch.StartNew();
        var fx = f(x0);

        if (HasDiverged(x0, fx))
        {
            return Divergence("Iterações divergiram no método de Newton.", x0, 0, stopwatch);
        }

        if (Math.Abs(fx) <= request.Tolerance)
        {
            return Success(x0, 0, stopwatch);
        }

        var iteration = 1;
        var derivativeAtX0 = derivative?.Invoke(x0) ?? ApproximateDerivative(f, x0);
        if (Math.Abs(derivativeAtX0) < SmallNumber)
        {
            return Divergence("Derivada próxima de zero no método de Newton.", x0, iteration, stopwatch);
        }

        var x1 = x0 - fx / derivativeAtX0;
        fx = f(x1);

        if (HasDiverged(x1, fx))
        {
            return Divergence("Iterações divergiram no método de Newton.", x1, iteration, stopwatch);
        }

        while (Math.Abs(fx) > request.Tolerance && Math.Abs(x1 - x0) > request.Tolerance && iteration <= request.MaxIterations)
        {
            iteration++;
            x0 = x1;

            var derivativeAtX = derivative?.Invoke(x0) ?? ApproximateDerivative(f, x0);
            if (Math.Abs(derivativeAtX) < SmallNumber)
            {
                return Divergence("Derivada próxima de zero no método de Newton.", x0, iteration, stopwatch);
            }

            var fxAtX0 = f(x0);
            x1 = x0 - fxAtX0 / derivativeAtX;
            fx = f(x1);

            if (HasDiverged(x1, fx))
            {
                return Divergence("Iterações divergiram no método de Newton.", x1, iteration, stopwatch);
            }
        }

        if (Math.Abs(fx) > request.Tolerance && Math.Abs(x1 - x0) > request.Tolerance && iteration > request.MaxIterations)
        {
            return MaxIterations(x1, iteration, stopwatch);
        }

        return Success(x1, iteration, stopwatch);
    }

    private RootFindingResult SolveWithFixedPoint(Func<double, double> f, Func<double, double> phi, RootFindingRequest request)
    {
        if (!request.InitialGuess.HasValue)
        {
            return Invalid("É necessário fornecer um chute inicial para o método do ponto fixo.");
        }

        var x0 = request.InitialGuess.Value;
        var stopwatch = Stopwatch.StartNew();
        var fx0 = f(x0);

        if (Math.Abs(fx0) < request.Tolerance)
        {
            return Success(x0, 0, stopwatch);
        }

        var iteration = 1;

        while (true)
        {
            var x1 = phi(x0);
            var fx1 = f(x1);

            if (HasDiverged(x1, fx1))
            {
                return Divergence("Iterações divergiram no método do ponto fixo.", x1, iteration, stopwatch);
            }

            if (Math.Abs(fx1) < request.Tolerance || Math.Abs(x1 - x0) < request.Tolerance || iteration > request.MaxIterations)
            {
                return iteration > request.MaxIterations
                    ? MaxIterations(x1, iteration, stopwatch)
                    : Success(x1, iteration, stopwatch);
            }

            x0 = x1;
            iteration++;
        }
    }

    private static double ApproximateDerivative(Func<double, double> f, double x)
    {
        var h = DerivativeStepScale * Math.Max(1.0, Math.Abs(x));
        var forward = f(x + h);
        var backward = f(x - h);
        return (forward - backward) / (2 * h);
    }

    private static bool HasDiverged(double value, double fx)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || double.IsNaN(fx) || double.IsInfinity(fx))
        {
            return true;
        }

        return Math.Abs(value) > DivergenceThreshold || Math.Abs(fx) > DivergenceThreshold;
    }

    private static RootFindingResult Invalid(string message)
    {
        return new RootFindingResult
        {
            Status = SolverStatus.InvalidInput,
            Message = message,
            Root = null,
            Iterations = 0,
            ElapsedMs = 0
        };
    }

    private static RootFindingResult Success(double root, int iterations, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        return new RootFindingResult
        {
            Status = SolverStatus.Success,
            Root = root,
            Iterations = iterations,
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
            Message = string.Empty
        };
    }

    private static RootFindingResult Divergence(string message, double? root, int iterations, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        return new RootFindingResult
        {
            Status = SolverStatus.Divergence,
            Message = message,
            Root = root,
            Iterations = iterations,
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds
        };
    }

    private static RootFindingResult MaxIterations(double root, int iterations, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        return new RootFindingResult
        {
            Status = SolverStatus.MaxIterationsReached,
            Message = "Número máximo de iterações atingido sem convergência na tolerância especificada.",
            Root = root,
            Iterations = iterations,
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds
        };
    }
}
