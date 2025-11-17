namespace NumericalMethods.Core.RootFinding;

public sealed class RootFindingRequest
{
    public string FunctionExpression { get; set; } = string.Empty;
    public string? PhiExpression { get; set; }
    public string? DerivativeExpression { get; set; }
    public RootFindingMethod Method { get; set; }
    public double? A { get; set; }
    public double? B { get; set; }
    public double? InitialGuess { get; set; }
    public double? SecondGuess { get; set; }
    public double Tolerance { get; set; }
    public int MaxIterations { get; set; }
}
