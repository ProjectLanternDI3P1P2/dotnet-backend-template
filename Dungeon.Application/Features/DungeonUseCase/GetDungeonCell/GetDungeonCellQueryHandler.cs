using Dungeon.Application.Models;
using Dungeon.Application.Ports;
using Dungeon.Domain.Entities;
using Dungeon.Domain.Enums;
using Dungeon.Domain.Repositories;
using Dungeon.Domain.ValueObjects;
using MediatR;

namespace Dungeon.Application.Features.DungeonUseCase.GetDungeonCell;

public sealed class GetDungeonCellQueryHandler(
    IDungeonRunRepository dungeonRunRepository,
    IDungeonProvider dungeonProvider
) : IRequestHandler<GetDungeonCellQuery, GetDungeonCellResult>
{
    public async Task<GetDungeonCellResult> Handle(
        GetDungeonCellQuery request,
        CancellationToken cancellationToken
    )
    {
        (GeneratedDungeon dungeon, DungeonFloor floor) = await KnownDungeon.GetFloorAsync(
            request.Seed,
            request.Floor,
            dungeonRunRepository,
            dungeonProvider,
            cancellationToken
        );

        // The validator guarantees both coordinates are present.
        Position position = new(request.X!.Value, request.Y!.Value);
        if (!floor.Contains(position))
        {
            throw new KeyNotFoundException(
                $"Cell ({position.X}, {position.Y}) is outside the {floor.Width}x{floor.Height} floor {floor.Index} of dungeon '{dungeon.Seed}'."
            );
        }

        CellType cellType = floor.GetCell(position);

        return new GetDungeonCellResult
        {
            Seed = dungeon.Seed.ToString(),
            Floor = floor.Index,
            X = position.X,
            Y = position.Y,
            Type = DungeonContract.Name(cellType),
            IsWalkable = cellType.IsWalkable(),
            RoomId = floor.GetRoomId(position),
            Elements = floor.GetElementsAt(position).Select(DungeonElementResult.From).ToList(),
        };
    }
}
