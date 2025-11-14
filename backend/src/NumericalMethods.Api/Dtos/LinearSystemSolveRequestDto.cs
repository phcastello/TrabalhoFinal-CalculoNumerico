using NumericalMethods.Core.LinearSystems;

namespace NumericalMethods.Api.Dtos;

public sealed class LinearSystemSolveRequestDto
{
    public double[][] A { get; set; } = Array.Empty<double[]>();
    public double[] B { get; set; } = Array.Empty<double>();
    public LinearSolverMethod Method { get; set; }
    public IterativeParamsDto? IterativeParams { get; set; }
    public bool ReturnSteps { get; set; }
}
