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

        return request.Method switch
        {
            RootFindingMethod.Bisection => SolveWithBisection(function, request),
            RootFindingMethod.FixedPoint => SolveWithFixedPoint(function, request),
            RootFindingMethod.Newton => SolveWithNewton(function, request),
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
        double fc = double.NaN;

        for (var iteration = 1; iteration <= request.MaxIterations; iteration++)
        {
            c = (a + b) / 2.0;
            fc = f(c);

            if (HasDiverged(c, fc))
            {
                stopwatch.Stop();
                return Divergence($"Iterações divergiram no método da Bisseção.", c, iteration, stopwatch);
            }

            if (Math.Abs(fc) < request.Tolerance || Math.Abs(b - a) / 2 < request.Tolerance)
            {
                stopwatch.Stop();
                return Success(c, iteration, stopwatch);
            }

            if (fa * fc < 0)
            {
                b = c;
                fb = fc;
            }
            else
            {
                a = c;
                fa = fc;
            }
        }

        stopwatch.Stop();
        return MaxIterations(c, request.MaxIterations, stopwatch);
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

        double c = double.NaN;
        double fc = double.NaN;

        for (var iteration = 1; iteration <= request.MaxIterations; iteration++)
        {
            var denominator = fb - fa;
            if (Math.Abs(denominator) < SmallNumber)
            {
                stopwatch.Stop();
                return Divergence("Denominador próximo de zero em Regula Falsi.", c, iteration, stopwatch);
            }

            c = (a * fb - b * fa) / denominator;
            fc = f(c);

            if (HasDiverged(c, fc))
            {
                stopwatch.Stop();
                return Divergence("Iterações divergiram em Regula Falsi.", c, iteration, stopwatch);
            }

            if (Math.Abs(fc) < request.Tolerance)
            {
                stopwatch.Stop();
                return Success(c, iteration, stopwatch);
            }

            if (fa * fc < 0)
            {
                b = c;
                fb = fc;
            }
            else
            {
                a = c;
                fa = fc;
            }
        }

        stopwatch.Stop();
        return MaxIterations(c, request.MaxIterations, stopwatch);
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

        for (var iteration = 1; iteration <= request.MaxIterations; iteration++)
        {
            var denominator = f1 - f0;
            if (Math.Abs(denominator) < SmallNumber)
            {
                stopwatch.Stop();
                return Divergence("Denominador próximo de zero no método da Secante.", x1, iteration, stopwatch);
            }

            var x2 = x1 - f1 * (x1 - x0) / denominator;
            var f2 = f(x2);

            if (HasDiverged(x2, f2))
            {
                stopwatch.Stop();
                return Divergence("Iterações divergiram no método da Secante.", x2, iteration, stopwatch);
            }

            if (Math.Abs(f2) < request.Tolerance || Math.Abs(x2 - x1) < request.Tolerance)
            {
                stopwatch.Stop();
                return Success(x2, iteration, stopwatch);
            }

            x0 = x1;
            f0 = f1;
            x1 = x2;
            f1 = f2;
        }

        stopwatch.Stop();
        return MaxIterations(x1, request.MaxIterations, stopwatch);
    }

    private RootFindingResult SolveWithNewton(Func<double, double> f, RootFindingRequest request)
    {
        if (!request.InitialGuess.HasValue)
        {
            return Invalid("É necessário fornecer um chute inicial para o método de Newton.");
        }

        var x = request.InitialGuess.Value;
        var stopwatch = Stopwatch.StartNew();

        for (var iteration = 1; iteration <= request.MaxIterations; iteration++)
        {
            var fx = f(x);
            if (HasDiverged(x, fx))
            {
                stopwatch.Stop();
                return Divergence("Iterações divergiram no método de Newton.", x, iteration, stopwatch);
            }

            if (Math.Abs(fx) < request.Tolerance)
            {
                stopwatch.Stop();
                return Success(x, iteration, stopwatch);
            }

            var derivative = ApproximateDerivative(f, x);
            if (Math.Abs(derivative) < SmallNumber)
            {
                stopwatch.Stop();
                return Divergence("Derivada próxima de zero no método de Newton.", x, iteration, stopwatch);
            }

            var next = x - fx / derivative;

            if (Math.Abs(next - x) < request.Tolerance)
            {
                stopwatch.Stop();
                return Success(next, iteration, stopwatch);
            }

            x = next;
        }

        stopwatch.Stop();
        return MaxIterations(x, request.MaxIterations, stopwatch);
    }

    private RootFindingResult SolveWithFixedPoint(Func<double, double> g, RootFindingRequest request)
    {
        if (!request.InitialGuess.HasValue)
        {
            return Invalid("É necessário fornecer um chute inicial para o método do ponto fixo.");
        }

        var x = request.InitialGuess.Value;
        var stopwatch = Stopwatch.StartNew();

        for (var iteration = 1; iteration <= request.MaxIterations; iteration++)
        {
            var next = g(x);

            if (HasDiverged(next, next - x))
            {
                stopwatch.Stop();
                return Divergence("Iterações divergiram no método do ponto fixo.", next, iteration, stopwatch);
            }

            if (Math.Abs(next - x) < request.Tolerance)
            {
                stopwatch.Stop();
                return Success(next, iteration, stopwatch);
            }

            x = next;
        }

        stopwatch.Stop();
        return MaxIterations(x, request.MaxIterations, stopwatch);
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
