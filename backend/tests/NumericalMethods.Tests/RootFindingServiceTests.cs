using NumericalMethods.Core.Common;
using NumericalMethods.Core.RootFinding;
using NumericalMethods.Core.Services;
using Xunit;

namespace NumericalMethods.Tests;

public class RootFindingServiceTests
{
    private readonly RootFindingService _service = new();

    [Fact]
    public void FixedPoint_WithInvalidPhiEvaluation_ReturnsInvalidWithoutIterations()
    {
        var request = new RootFindingRequest
        {
            FunctionExpression = "x^2 - 4",
            PhiExpression = "1/(x-1)", // undefined at x = 1
            Method = RootFindingMethod.FixedPoint,
            InitialGuess = 1,
            Tolerance = 1e-4,
            MaxIterations = 5
        };

        var result = _service.Solve(request, returnSteps: true);

        Assert.Equal(SolverStatus.InvalidInput, result.Status);
        Assert.Null(result.Root);
        Assert.Equal(0, result.Iterations);
    }

    [Fact]
    public void Newton_WhenInitialGuessMeetsTolerance_RegistersInitialStep()
    {
        var request = new RootFindingRequest
        {
            FunctionExpression = "x*log10(x) - 1",
            DerivativeExpression = "3*x^2 - 9",
            Method = RootFindingMethod.Newton,
            InitialGuess = 2.5,
            Tolerance = 0.01,
            MaxIterations = 100
        };

        var result = _service.Solve(request, returnSteps: true);

        Assert.Equal(SolverStatus.Success, result.Status);
        Assert.Equal(0, result.Iterations);
        Assert.NotNull(result.Steps);
        Assert.NotEmpty(result.Steps!);
        Assert.Equal(2.5, result.Steps![0].X, 3);
    }

    [Fact]
    public void Bisection_AcceptsIntervalWithOppositeSigns()
    {
        var request = new RootFindingRequest
        {
            FunctionExpression = "e^(-x^(2)) - cos(x)",
            Method = RootFindingMethod.Bisection,
            A = 1,
            B = 2,
            Tolerance = 0.1,
            MaxIterations = 100
        };

        var result = _service.Solve(request, returnSteps: true);

        Assert.Equal(SolverStatus.Success, result.Status);
        Assert.NotNull(result.Root);
        Assert.True(result.Iterations > 0);
    }

    [Fact]
    public void MaxIterationsResult_DoesNotExposeRoot()
    {
        var request = new RootFindingRequest
        {
            FunctionExpression = "x^2 - 2",
            Method = RootFindingMethod.RegulaFalsi,
            A = 0,
            B = 2,
            Tolerance = 1e-12,
            MaxIterations = 1
        };

        var result = _service.Solve(request, returnSteps: true);

        Assert.Equal(SolverStatus.MaxIterationsReached, result.Status);
        Assert.Null(result.Root);
    }
}
