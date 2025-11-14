using NumericalMethods.Core.RootFinding;

namespace NumericalMethods.Api.Dtos;

public sealed class RootFindingSolveRequestDto
{
    public string FunctionExpression { get; set; } = string.Empty;
    public RootFindingMethod Method { get; set; }
    public double? A { get; set; }
    public double? B { get; set; }
    public double? InitialGuess { get; set; }
    public double? SecondGuess { get; set; }
    public double Tolerance { get; set; }
    public int MaxIterations { get; set; }
    public bool ReturnSteps { get; set; }
}
