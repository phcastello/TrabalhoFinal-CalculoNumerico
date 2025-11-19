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
            return Invalid("Expressão inválida para φ(x).");
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
                return Invalid("É É necessário fornecer φ(x) para o método do ponto fixo.");
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
                return Invalid("Expressão inválida para φ(x).");
            }
            catch (Exception)
            {
                return Invalid("Não foi possível interpretar a expressão fornecida.para f'(x).");
            }
        }

        return request.Method switch
        {
            RootFindingMethod.Bisection => SolveWithBisection(function, request, returnSteps),
            RootFindingMethod.FixedPoint => SolveWithFixedPoint(function, phi!, request, returnSteps),
            RootFindingMethod.Newton => SolveWithNewton(function, derivative, request, returnSteps),
            RootFindingMethod.RegulaFalsi => SolveWithRegulaFalsi(function, request, returnSteps),
            RootFindingMethod.Secant => SolveWithSecant(function, request, returnSteps),
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

    private RootFindingResult SolveWithBisection(Func<double, double> f, RootFindingRequest request, bool returnSteps)
    {
        if (!request.A.HasValue || !request.B.HasValue)
        {
            return Invalid("Parâmetros 'a' e 'b' são obrigatórios para a Bisseção.");
        }

        var a = request.A.Value;
        var b = request.B.Value;
        if (a >= b)
        {
            return Invalid("É É necessário que 'a' seja menor que 'b' para a Bisseção.");
        }

        var stopwatch = Stopwatch.StartNew();
        var steps = returnSteps ? new List<RootFindingStep>() : null;
        var fa = f(a);
        var fb = f(b);

        if (fa * fb > 0)
        {
            stopwatch.Stop();
            return Invalid("f(a) e f(b) devem ter sinais opostos para a Bisseção.", steps);
        }

        double c = double.NaN;

        var iterationCount = 0;
        if (Math.Abs(b - a) < request.Tolerance)
        {
            return Success((a + b) / 2.0, iterationCount, stopwatch, steps);
        }

        while (Math.Abs(b - a) > request.Tolerance && iterationCount < request.MaxIterations)
        {
            iterationCount++;

            var finicio = f(a);
            var meio = (a + b) / 2.0;
            var fmeio = f(meio);
            var currentError = Math.Abs(b - a);

            if (HasDiverged(meio, fmeio))
            {
                AddStep(steps, iterationCount, meio, fmeio, a, b, currentError);
                return Divergence($"Iterações divergiram no método da Bisseção.", meio, iterationCount, stopwatch, steps);
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

            currentError = Math.Abs(b - a);
            AddStep(steps, iterationCount, meio, fmeio, a, b, currentError);
        }

        c = (a + b) / 2.0;
        if (Math.Abs(b - a) > request.Tolerance)
        {
            return MaxIterations(c, iterationCount, stopwatch, steps);
        }

        return Success(c, iterationCount, stopwatch, steps);
    }

    private RootFindingResult SolveWithRegulaFalsi(Func<double, double> f, RootFindingRequest request, bool returnSteps)
    {
        if (!request.A.HasValue || !request.B.HasValue)
        {
            return Invalid("Parâmetros 'a' e 'b' são obrigatórios para Regula Falsi.");
        }

        var a = request.A.Value;
        var b = request.B.Value;
        if (a >= b)
        {
            return Invalid("É É necessário que 'a' seja menor que 'b' para Regula Falsi.");
        }

        var stopwatch = Stopwatch.StartNew();
        var steps = returnSteps ? new List<RootFindingStep>() : null;
        var fa = f(a);
        var fb = f(b);

        if (fa * fb > 0)
        {
            stopwatch.Stop();
            return Invalid("f(a) e f(b) devem ter sinais opostos para Regula Falsi.", steps);
        }

        var delta1 = request.Tolerance;
        var delta2 = request.Tolerance;

        if (Math.Abs(b - a) < delta1)
        {
            return Success((a + b) / 2.0, 0, stopwatch, steps);
        }

        if (Math.Abs(fa) < delta2)
        {
            return Success(a, 0, stopwatch, steps);
        }

        if (Math.Abs(fb) < delta2)
        {
            return Success(b, 0, stopwatch, steps);
        }

        var iteration = 1;

        while (true)
        {
            var M = fa;
            var denominator = fb - fa;
            if (Math.Abs(denominator) < SmallNumber)
            {
                AddStep(steps, iteration, double.NaN, double.NaN, a, b, Math.Abs(b - a));
                return Divergence("Denominador próximo de zero em Regula Falsi.", double.NaN, iteration, stopwatch, steps);
            }

            var x = (a * fb - b * fa) / denominator;
            var fx = f(x);

            if (HasDiverged(x, fx))
            {
                AddStep(steps, iteration, x, fx, a, b, Math.Abs(b - a));
                return Divergence("Iterações divergiram em Regula Falsi.", x, iteration, stopwatch, steps);
            }

            if (Math.Abs(fx) < delta2 || iteration > request.MaxIterations)
            {
                AddStep(steps, iteration, x, fx, a, b, Math.Abs(b - a));
                return iteration > request.MaxIterations
                    ? MaxIterations(x, iteration, stopwatch, steps)
                    : Success(x, iteration, stopwatch, steps);
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

            AddStep(steps, iteration, x, fx, a, b, Math.Abs(b - a));

            if (Math.Abs(b - a) < delta1)
            {
                var midpoint = (a + b) / 2.0;
                return Success(midpoint, iteration, stopwatch, steps);
            }

            iteration++;
        }
    }

    private RootFindingResult SolveWithSecant(Func<double, double> f, RootFindingRequest request, bool returnSteps)
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
            return Invalid("É É necessário fornecer dois chutes iniciais para a Secante.");
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
        var steps = returnSteps ? new List<RootFindingStep>() : null;

        if (Math.Abs(f0) < request.Tolerance)
        {
            return Success(x0, 0, stopwatch, steps);
        }

        if (Math.Abs(f1) < request.Tolerance || Math.Abs(x1 - x0) < request.Tolerance)
        {
            return Success(x1, 0, stopwatch, steps);
        }

        var iteration = 1;

        while (true)
        {
            var denominator = f1 - f0;
            if (Math.Abs(denominator) < SmallNumber)
            {
                AddStep(steps, iteration, x1, f1, null, null, Math.Abs(x1 - x0));
                return Divergence("Denominador próximo de zero no método da Secante.", x1, iteration, stopwatch, steps);
            }

            var x2 = x1 - f1 * (x1 - x0) / denominator;
            var f2 = f(x2);
            var error = Math.Abs(x2 - x1);

            if (HasDiverged(x2, f2))
            {
                AddStep(steps, iteration, x2, f2, null, null, error);
                return Divergence("Iterações divergiram no método da Secante.", x2, iteration, stopwatch, steps);
            }

            AddStep(steps, iteration, x2, f2, null, null, error);

            if (Math.Abs(f2) < request.Tolerance || error < request.Tolerance || iteration > request.MaxIterations)
            {
                return iteration > request.MaxIterations
                    ? MaxIterations(x2, iteration, stopwatch, steps)
                    : Success(x2, iteration, stopwatch, steps);
            }

            x0 = x1;
            f0 = f1;
            x1 = x2;
            f1 = f2;
            iteration++;
        }
    }

    private RootFindingResult SolveWithNewton(Func<double, double> f, Func<double, double>? derivative, RootFindingRequest request, bool returnSteps)
    {
        if (!request.InitialGuess.HasValue)
        {
            return Invalid("É necessário fornecer um chute inicial para o método de Newton.");
        }

        var x0 = request.InitialGuess.Value;
        var stopwatch = Stopwatch.StartNew();
        var steps = returnSteps ? new List<RootFindingStep>() : null;

        if (!TryEvaluateFunction(f, x0, out var fx0, out var fx0Error))
        {
            AddStep(steps, 0, x0, double.NaN);
            return Invalid($"Não foi possível avaliar f(x) no chute inicial: {fx0Error}", steps);
        }

        AddStep(steps, 0, x0, fx0, null, null, 0);

        if (HasDiverged(x0, fx0))
        {
            return Divergence("Iterações divergiram no método de Newton.", null, 0, stopwatch, steps);
        }

        if (Math.Abs(fx0) <= request.Tolerance)
        {
            return Success(x0, 0, stopwatch, steps);
        }

        var iteration = 1;
        var currentFx = fx0;

        while (true)
        {
            var derivativeAtX = derivative?.Invoke(x0) ?? ApproximateDerivative(f, x0);
            if (Math.Abs(derivativeAtX) < SmallNumber)
            {
                return Divergence("Derivada próxima de zero no método de Newton.", null, iteration, stopwatch, steps);
            }

            var x1 = x0 - currentFx / derivativeAtX;
            if (!TryEvaluateFunction(f, x1, out var fx1, out var fx1Error))
            {
                AddStep(steps, iteration, x1, double.NaN, null, null, Math.Abs(x1 - x0));
                return Divergence($"Não foi possível avaliar f(x) no método de Newton: {fx1Error}", null, iteration, stopwatch, steps);
            }

            var error = Math.Abs(x1 - x0);

            if (HasDiverged(x1, fx1))
            {
                AddStep(steps, iteration, x1, fx1, null, null, error);
                return Divergence("Iterações divergiram no método de Newton.", null, iteration, stopwatch, steps);
            }

            AddStep(steps, iteration, x1, fx1, null, null, error);

            if (Math.Abs(fx1) <= request.Tolerance || error <= request.Tolerance)
            {
                return Success(x1, iteration, stopwatch, steps);
            }

            if (iteration >= request.MaxIterations)
            {
                return MaxIterations(null, iteration, stopwatch, steps);
            }

            x0 = x1;
            currentFx = fx1;
            iteration++;
        }
    }

    private RootFindingResult SolveWithFixedPoint(Func<double, double> f, Func<double, double> phi, RootFindingRequest request, bool returnSteps)
    {
        if (!request.InitialGuess.HasValue)
        {
            return Invalid("É É necessário fornecer um chute inicial para o método do ponto fixo.");
        }

        var x0 = request.InitialGuess.Value;
        var stopwatch = Stopwatch.StartNew();
        var steps = returnSteps ? new List<RootFindingStep>() : null;

        if (!TryEvaluateFunction(f, x0, out var fx0, out var fx0Error))
        {
            AddStep(steps, 0, x0, double.NaN);
            return Invalid($"Não foi possível avaliar f(x) no chute inicial: {fx0Error}", steps);
        }

        AddStep(steps, 0, x0, fx0, null, null, 0);

        if (!TryEvaluateFunction(phi, x0, out var nextGuess, out var phiError))
        {
            return Invalid("Função φ(x) inválida ou não pôde ser avaliada no chute inicial.", steps);
        }

        if (Math.Abs(fx0) < request.Tolerance)
        {
            return Success(x0, 0, stopwatch, steps);
        }

        var iteration = 1;

        while (true)
        {
            if (!TryEvaluateFunction(f, nextGuess, out var fx1, out var fx1Error))
            {
                AddStep(steps, iteration, nextGuess, double.NaN, null, null, Math.Abs(nextGuess - x0));
                return Divergence($"Não foi possível avaliar f(x) no método do ponto fixo: {fx1Error}", null, iteration, stopwatch, steps);
            }

            var error = Math.Abs(nextGuess - x0);

            if (HasDiverged(nextGuess, fx1))
            {
                AddStep(steps, iteration, nextGuess, fx1, null, null, error);
                return Divergence("Iterações divergiram no método do ponto fixo.", null, iteration, stopwatch, steps);
            }

            AddStep(steps, iteration, nextGuess, fx1, null, null, error);

            if (Math.Abs(fx1) < request.Tolerance || error < request.Tolerance)
            {
                return Success(nextGuess, iteration, stopwatch, steps);
            }

            if (iteration >= request.MaxIterations)
            {
                return MaxIterations(null, iteration, stopwatch, steps);
            }

            x0 = nextGuess;

            if (!TryEvaluateFunction(phi, x0, out nextGuess, out phiError))
            {
                return Invalid("Função φ(x) inválida ou não pôde ser avaliada durante as iterações.", steps);
            }

            iteration++;
        }
    }

    private static bool TryEvaluateFunction(Func<double, double> function, double x, out double value, out string errorMessage)
    {
        try
        {
            value = function(x);
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                errorMessage = "Resultado não numérico para a expressão.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
        catch (ExpressionParseException ex)
        {
            value = double.NaN;
            errorMessage = ex.Message;
            return false;
        }
        catch (Exception)
        {
            value = double.NaN;
            errorMessage = "Não foi possível avaliar a expressão.";
            return false;
        }
    }

    private static void AddStep(
        List<RootFindingStep>? steps,
        int iteration,
        double x,
        double fx,
        double? a = null,
        double? b = null,
        double? error = null)
    {
        if (steps is null)
        {
            return;
        }

        steps.Add(new RootFindingStep
        {
            Iteration = iteration,
            X = x,
            Fx = fx,
            A = a,
            B = b,
            Error = error
        });
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

    private static RootFindingResult Invalid(string message, List<RootFindingStep>? steps = null)
    {
        return new RootFindingResult
        {
            Status = SolverStatus.InvalidInput,
            Message = message,
            Root = null,
            Iterations = 0,
            ElapsedMs = 0,
            Steps = steps
        };
    }

    private static RootFindingResult Success(double root, int iterations, Stopwatch stopwatch, List<RootFindingStep>? steps = null)
    {
        stopwatch.Stop();
        return new RootFindingResult
        {
            Status = SolverStatus.Success,
            Root = root,
            Iterations = iterations,
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
            Message = string.Empty,
            Steps = steps
        };
    }

    private static RootFindingResult Divergence(string message, double? root, int iterations, Stopwatch stopwatch, List<RootFindingStep>? steps = null)
    {
        stopwatch.Stop();
        return new RootFindingResult
        {
            Status = SolverStatus.Divergence,
            Message = message,
            Root = null,
            Iterations = iterations,
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
            Steps = steps
        };
    }

    private static RootFindingResult MaxIterations(double? root, int iterations, Stopwatch stopwatch, List<RootFindingStep>? steps = null)
    {
        stopwatch.Stop();
        return new RootFindingResult
        {
            Status = SolverStatus.MaxIterationsReached,
            Message = "Número máximo de iterações atingido sem convergência na tolerância especificada.",
            Root = null,
            Iterations = iterations,
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
            Steps = steps
        };
    }
}


