using NumericalMethods.Api.Dtos;
using NumericalMethods.Core.LinearSystems;

namespace NumericalMethods.Api.Mapping;

public static class LinearSystemMappings
{
    public static LinearSystem ToDomain(this LinearSystemSolveRequestDto dto)
    {
        var matrix = ConvertToMatrix(dto.A);
        return new LinearSystem(matrix, dto.B);
    }

    public static IterativeParams? ToDomain(this IterativeParamsDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        return new IterativeParams
        {
            Tolerance = dto.Tolerance,
            MaxIterations = dto.MaxIterations,
            StopCondition = dto.StopCondition
        };
    }

    public static LinearSystemSolveResponseDto ToDto(this SolverResult result)
    {
        return new LinearSystemSolveResponseDto
        {
            Status = result.Status,
            Solution = result.Solution,
            Iterations = result.Iterations,
            ElapsedMs = result.ElapsedMs,
            Message = result.Message
        };
    }

    private static double[,] ConvertToMatrix(double[][] source)
    {
        if (source.Length == 0)
        {
            return new double[0, 0];
        }

        var rows = source.Length;
        var cols = source[0]?.Length ?? 0;

        if (cols == 0)
        {
            return new double[rows, 0];
        }

        var matrix = new double[rows, cols];

        for (var i = 0; i < rows; i++)
        {
            var row = source[i] ?? throw new ArgumentException("Matrix A rows cannot be null.", nameof(source));

            if (row.Length != cols)
            {
                throw new ArgumentException("Matrix A must be rectangular.", nameof(source));
            }

            for (var j = 0; j < cols; j++)
            {
                matrix[i, j] = row[j];
            }
        }

        return matrix;
    }
}
