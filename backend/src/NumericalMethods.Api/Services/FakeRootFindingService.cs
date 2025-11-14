using NumericalMethods.Core.Common;
using NumericalMethods.Core.RootFinding;
using NumericalMethods.Core.Services;

namespace NumericalMethods.Api.Services;

public sealed class FakeRootFindingService : IRootFindingService
{
    public RootFindingResult Solve(RootFindingRequest request, bool returnSteps = false)
    {
        return new RootFindingResult
        {
            Status = SolverStatus.NotImplemented,
            Root = null,
            Iterations = 0,
            ElapsedMs = 0,
            Message = "Métodos de raiz ainda não implementados nesta etapa."
        };
    }
}
