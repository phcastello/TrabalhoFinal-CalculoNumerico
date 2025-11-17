using NumericalMethods.Core.Common;

namespace NumericalMethods.Core.RootFinding;

public sealed class RootFindingResult
{
    public SolverStatus Status { get; set; }
    public double? Root { get; set; }
    public int Iterations { get; set; }
    public double ElapsedMs { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<RootFindingStep>? Steps { get; set; }
}
