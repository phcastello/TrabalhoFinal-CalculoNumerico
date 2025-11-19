using NumericalMethods.Core.Common;
using NumericalMethods.Core.LinearSystems;
using NumericalMethods.Core.Services;
using Xunit;

namespace NumericalMethods.Tests;

public class LinearSystemSolverServiceTests
{
    private readonly LinearSystemSolverService _service = new();

    private static readonly double[,] MatrixA = new double[,]
    {
        { 2, -1, 3, 5 },
        { 6, -3, 12, 11 },
        { 4, -1, 10, 8 },
        { 0, -2, -8, 10 }
    };

    private static readonly double[] VectorB = new[] { -7d, 4d, 4d, -60d };

    private static readonly double[] ExpectedSolution = new[] { 1d, -2d, 3d, -4d };

    [Fact]
    public void Gauss_WithPivotFallback_SolvesSystem()
    {
        var system = new LinearSystem(MatrixA, VectorB);
        var result = _service.Solve(system, LinearSolverMethod.Gauss);

        Assert.Equal(SolverStatus.Success, result.Status);
        AssertSolutionMatches(ExpectedSolution, result.Solution, 6);
    }

    [Fact]
    public void Lu_WithPivoting_SolvesSystem()
    {
        var system = new LinearSystem(MatrixA, VectorB);
        var result = _service.Solve(system, LinearSolverMethod.LU);

        Assert.Equal(SolverStatus.Success, result.Status);
        AssertSolutionMatches(ExpectedSolution, result.Solution, 6);
    }

    private static void AssertSolutionMatches(double[] expected, double[] actual, int precision)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], actual[i], precision);
        }
    }
}
