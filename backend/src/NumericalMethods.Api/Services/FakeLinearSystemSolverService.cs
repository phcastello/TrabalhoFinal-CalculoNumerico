using NumericalMethods.Core.Common;
using NumericalMethods.Core.LinearSystems;
using NumericalMethods.Core.Services;

namespace NumericalMethods.Api.Services;

public sealed class FakeLinearSystemSolverService : ILinearSystemSolverService
{
    public SolverResult Solve(
        LinearSystem system,
        LinearSolverMethod method,
        IterativeParams? iterativeParams = null,
        bool returnSteps = false)
    {
        return new SolverResult
        {
            Status = SolverStatus.NotImplemented,
            Solution = new double[system.N],
            Iterations = 0,
            ElapsedMs = 0,
            Message = "Métodos numéricos ainda não implementados nesta etapa."
        };
    }
}
