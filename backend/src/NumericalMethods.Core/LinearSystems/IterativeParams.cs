namespace NumericalMethods.Core.LinearSystems;

public sealed class IterativeParams
{
    public double Tolerance { get; set; }
    public int MaxIterations { get; set; }
    public IterativeStopCondition StopCondition { get; set; }
}
