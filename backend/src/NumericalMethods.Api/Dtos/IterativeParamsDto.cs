using NumericalMethods.Core.LinearSystems;

namespace NumericalMethods.Api.Dtos;

public sealed class IterativeParamsDto
{
    public double Tolerance { get; set; }
    public int MaxIterations { get; set; }
    public IterativeStopCondition StopCondition { get; set; }
}
