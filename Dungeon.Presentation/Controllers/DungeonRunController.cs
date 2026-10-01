using Dungeon.Application.Features.DungeonRunUseCase.CreateDungeonRun;
using Dungeon.Application.Features.DungeonRunUseCase.DefeatFloorBoss;
using Dungeon.Application.Features.DungeonRunUseCase.GetDungeonRunById;
using Dungeon.Application.Features.DungeonRunUseCase.MoveHero;
using Dungeon.Presentation.DTO;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Dungeon.Presentation.Controllers;

[ApiController]
[Route("api/v1/dungeon-runs")]
public sealed class DungeonRunController(IMediator mediator, ILogger logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDungeonRunDto createDungeonRunDto,
        CancellationToken cancellationToken
    )
    {
        Guid runId = createDungeonRunDto.RunId ?? Guid.CreateVersion7();
        logger.Information(
            "Received request to create dungeon run {RunId} for game session {GameSessionId}.",
            runId,
            createDungeonRunDto.GameSessionId
        );

        await mediator.Send(
            new CreateDungeonRunCommand(
                runId,
                createDungeonRunDto.GameSessionId,
                createDungeonRunDto.Seed
            ),
            cancellationToken
        );
        var run = await mediator.Send(new GetDungeonRunByIdQuery(runId), cancellationToken);

        logger.Information("Dungeon run {RunId} created with seed {Seed}.", runId, run.Seed);
        return CreatedAtAction(nameof(GetById), new { runId }, run);
    }

    [HttpGet("{runId:guid}")]
    public async Task<IActionResult> GetById(Guid runId, CancellationToken cancellationToken)
    {
        logger.Information("Received request to get dungeon run {RunId}.", runId);

        var run = await mediator.Send(new GetDungeonRunByIdQuery(runId), cancellationToken);

        return Ok(run);
    }

    /// <summary>
    /// Moves the hero one tile. Walking into the open gate of the boss room takes the party
    /// down to the next floor. 409 when the target tile is not walkable, or the gate still
    /// locked.
    /// </summary>
    [HttpPost("{runId:guid}/moves")]
    public async Task<IActionResult> Move(
        Guid runId,
        [FromBody] MoveHeroDto moveHeroDto,
        CancellationToken cancellationToken
    )
    {
        logger.Information(
            "Received request to move the hero of dungeon run {RunId} {Direction}.",
            runId,
            moveHeroDto.Direction
        );

        await mediator.Send(new MoveHeroCommand(runId, moveHeroDto.Direction), cancellationToken);
        var run = await mediator.Send(new GetDungeonRunByIdQuery(runId), cancellationToken);

        return Ok(run);
    }

    /// <summary>
    /// Records the defeat of the current floor's boss, which opens the gate down (or wins the
    /// run on the last floor). For Combat to call once the fight is won.
    /// 409 when the hero is not in the boss room.
    /// </summary>
    [HttpPost("{runId:guid}/boss-defeats")]
    public async Task<IActionResult> DefeatFloorBoss(
        Guid runId,
        CancellationToken cancellationToken
    )
    {
        logger.Information("Received boss defeat for dungeon run {RunId}.", runId);

        await mediator.Send(new DefeatFloorBossCommand(runId), cancellationToken);
        var run = await mediator.Send(new GetDungeonRunByIdQuery(runId), cancellationToken);

        logger.Information(
            "Dungeon run {RunId}: boss of floor {Floor} defeated, status {Status}.",
            runId,
            run.CurrentFloor,
            run.Status
        );
        return Ok(run);
    }
}
