namespace NumericalMethods.Core.Common;

public enum SolverStatus
{
    Success,
    SingularMatrix,
    NotSPD,
    Divergence,
    MaxIterationsReached,
    InvalidInput,
    NotImplemented
}
