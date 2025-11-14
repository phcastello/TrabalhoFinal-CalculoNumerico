using NumericalMethods.Core.LinearSystems;

namespace NumericalMethods.Core.Services;

public interface ILinearSystemSolverService
{
    SolverResult Solve(
        LinearSystem system,
        LinearSolverMethod method,
        IterativeParams? iterativeParams = null,
        bool returnSteps = false);
}
