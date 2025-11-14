using NumericalMethods.Core.Common;

namespace NumericalMethods.Api.Dtos;

public sealed class RootFindingSolveResponseDto
{
    public SolverStatus Status { get; set; }
    public double? Root { get; set; }
    public int Iterations { get; set; }
    public double ElapsedMs { get; set; }
    public string Message { get; set; } = string.Empty;
}
