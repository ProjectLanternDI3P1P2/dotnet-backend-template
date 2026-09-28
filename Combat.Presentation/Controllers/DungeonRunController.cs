using Combat.Application.Features.DungeonRunUseCase.CreateDungeonRun;
using Combat.Application.Features.DungeonRunUseCase.GetDungeonRunById;
using Combat.Application.Features.DungeonRunUseCase.MoveHero;
using Combat.Application.Features.DungeonRunUseCase.TakeStairsDown;
using Combat.Presentation.DTO;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Combat.Presentation.Controllers;

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

    /// <summary>Moves the hero one tile. 409 when the target tile is not walkable.</summary>
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

    /// <summary>Takes the stairs the hero stands on. 409 when there are none.</summary>
    [HttpPost("{runId:guid}/descents")]
    public async Task<IActionResult> Descend(Guid runId, CancellationToken cancellationToken)
    {
        logger.Information("Received request to descend in dungeon run {RunId}.", runId);

        await mediator.Send(new TakeStairsDownCommand(runId), cancellationToken);
        var run = await mediator.Send(new GetDungeonRunByIdQuery(runId), cancellationToken);

        logger.Information(
            "Dungeon run {RunId} reached floor {Floor}.",
            runId,
            run.CurrentFloor
        );
        return Ok(run);
    }
}
