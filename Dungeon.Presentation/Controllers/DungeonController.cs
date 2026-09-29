using Dungeon.Application.Features.DungeonUseCase.GetDungeonCell;
using Dungeon.Application.Features.DungeonUseCase.GetDungeonMap;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Dungeon.Presentation.Controllers;

/// <summary>
/// Read access to generated dungeons by seed, for the game client and for the Combat and
/// Inventory services (US-DUNGEON-02).
/// </summary>
[ApiController]
[Route("api/v1/dungeons")]
public sealed class DungeonController(IMediator mediator, ILogger logger) : ControllerBase
{
    // A known seed always produces the same floor, so the browser and the Gateway may keep it
    // for good. Error responses are not affected: the header is only set on success.
    private const string ImmutableCacheControl = "public, max-age=31536000, immutable";

    [HttpGet("{seed}/map")]
    public async Task<IActionResult> GetMap(
        string seed,
        [FromQuery] int floor,
        CancellationToken cancellationToken
    )
    {
        logger.Information("Received request to get floor {Floor} of dungeon {Seed}.", floor, seed);

        var map = await mediator.Send(new GetDungeonMapQuery(seed, floor), cancellationToken);

        Response.Headers.CacheControl = ImmutableCacheControl;
        return Ok(map);
    }

    [HttpGet("{seed}/cell")]
    public async Task<IActionResult> GetCell(
        string seed,
        [FromQuery] int? x,
        [FromQuery] int? y,
        [FromQuery] int floor,
        CancellationToken cancellationToken
    )
    {
        logger.Information(
            "Received request to get cell ({X}, {Y}) of floor {Floor} of dungeon {Seed}.",
            x,
            y,
            floor,
            seed
        );

        var cell = await mediator.Send(new GetDungeonCellQuery(seed, floor, x, y), cancellationToken);

        Response.Headers.CacheControl = ImmutableCacheControl;
        return Ok(cell);
    }
}
