using NumericalMethods.Core.Common;

namespace NumericalMethods.Core.LinearSystems;

public sealed class SolverResult
{
    public SolverStatus Status { get; set; }
    public double[] Solution { get; set; } = Array.Empty<double>();
    public int Iterations { get; set; }
    public double ElapsedMs { get; set; }
    public string Message { get; set; } = string.Empty;
}
