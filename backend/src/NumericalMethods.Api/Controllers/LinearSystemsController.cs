using Microsoft.AspNetCore.Mvc;
using NumericalMethods.Api.Dtos;
using NumericalMethods.Api.Mapping;
using NumericalMethods.Core.Services;

namespace NumericalMethods.Api.Controllers;

[ApiController]
[Route("api/linear-systems")]
public sealed class LinearSystemsController : ControllerBase
{
    private readonly ILinearSystemSolverService _solverService;

    public LinearSystemsController(ILinearSystemSolverService solverService)
    {
        _solverService = solverService;
    }

    [HttpPost("solve")]
    [ProducesResponseType(typeof(LinearSystemSolveResponseDto), StatusCodes.Status200OK)]
    public ActionResult<LinearSystemSolveResponseDto> Solve(LinearSystemSolveRequestDto request)
    {
        var system = request.ToDomain();
        var iterativeParams = request.IterativeParams.ToDomain();
        var solverResult = _solverService.Solve(system, request.Method, iterativeParams);
        return Ok(solverResult.ToDto());
    }
}
