using NumericalMethods.Core.Common;

namespace NumericalMethods.Api.Dtos;

public sealed class RootFindingStepDto
{
    public int Iteration { get; set; }
    public double X { get; set; }
    public double Fx { get; set; }
    public double? A { get; set; }
    public double? B { get; set; }
    public double? Error { get; set; }
}

public sealed class RootFindingSolveResponseDto
{
    public SolverStatus Status { get; set; }
    public double? Root { get; set; }
    public int Iterations { get; set; }
    public double ElapsedMs { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<RootFindingStepDto>? Steps { get; set; }
}
