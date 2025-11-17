namespace NumericalMethods.Core.RootFinding;

public sealed class RootFindingStep
{
    public int Iteration { get; init; }
    public double X { get; init; }
    public double Fx { get; init; }
    public double? A { get; init; }
    public double? B { get; init; }
    public double? Error { get; init; }
}
