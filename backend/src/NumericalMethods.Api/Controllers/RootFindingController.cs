using Microsoft.AspNetCore.Mvc;
using NumericalMethods.Api.Dtos;
using NumericalMethods.Api.Mapping;
using NumericalMethods.Core.Services;

namespace NumericalMethods.Api.Controllers;

[ApiController]
[Route("api/roots")]
public sealed class RootFindingController : ControllerBase
{
    private readonly IRootFindingService _rootFindingService;

    public RootFindingController(IRootFindingService rootFindingService)
    {
        _rootFindingService = rootFindingService;
    }

    [HttpPost("solve")]
    [ProducesResponseType(typeof(RootFindingSolveResponseDto), StatusCodes.Status200OK)]
    public ActionResult<RootFindingSolveResponseDto> Solve(RootFindingSolveRequestDto request)
    {
        var rootRequest = request.ToDomain();
        var result = _rootFindingService.Solve(rootRequest, request.ReturnSteps);
        return Ok(result.ToDto());
    }
}
