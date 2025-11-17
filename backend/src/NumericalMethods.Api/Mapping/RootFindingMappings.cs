using NumericalMethods.Api.Dtos;
using NumericalMethods.Core.RootFinding;

namespace NumericalMethods.Api.Mapping;

public static class RootFindingMappings
{
    public static RootFindingRequest ToDomain(this RootFindingSolveRequestDto dto)
    {
        return new RootFindingRequest
        {
            FunctionExpression = dto.FunctionExpression,
            PhiExpression = dto.PhiExpression,
            DerivativeExpression = dto.DerivativeExpression,
            Method = dto.Method,
            A = dto.A,
            B = dto.B,
            InitialGuess = dto.InitialGuess,
            SecondGuess = dto.SecondGuess,
            Tolerance = dto.Tolerance,
            MaxIterations = dto.MaxIterations
        };
    }

    public static RootFindingSolveResponseDto ToDto(this RootFindingResult result)
    {
        return new RootFindingSolveResponseDto
        {
            Status = result.Status,
            Root = result.Root,
            Iterations = result.Iterations,
            ElapsedMs = result.ElapsedMs,
            Message = result.Message,
            Steps = result.Steps?.Select(s => new RootFindingStepDto
            {
                Iteration = s.Iteration,
                X = s.X,
                Fx = s.Fx,
                A = s.A,
                B = s.B,
                Error = s.Error
            }).ToList()
        };
    }
}
