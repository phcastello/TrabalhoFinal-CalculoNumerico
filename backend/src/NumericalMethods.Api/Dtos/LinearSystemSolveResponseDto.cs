using NumericalMethods.Core.Common;

namespace NumericalMethods.Api.Dtos;

public sealed class LinearSystemSolveResponseDto
{
    public SolverStatus Status { get; set; }
    public double[] Solution { get; set; } = Array.Empty<double>();
    public int Iterations { get; set; }
    public double ElapsedMs { get; set; }
    public string Message { get; set; } = string.Empty;
}
