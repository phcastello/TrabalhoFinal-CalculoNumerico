using NumericalMethods.Core.RootFinding;

namespace NumericalMethods.Core.Services;

public interface IRootFindingService
{
    RootFindingResult Solve(
        RootFindingRequest request,
        bool returnSteps = false);
}
