using Dungeon.Application.Models;
using Dungeon.Application.Ports;
using Dungeon.Domain.Entities;
using Dungeon.Domain.Repositories;
using Dungeon.Domain.ValueObjects;
using MediatR;

namespace Dungeon.Application.Features.DungeonUseCase.GetDungeonMap;

public sealed class GetDungeonMapQueryHandler(
    IDungeonRunRepository dungeonRunRepository,
    IDungeonProvider dungeonProvider
) : IRequestHandler<GetDungeonMapQuery, GetDungeonMapResult>
{
    public async Task<GetDungeonMapResult> Handle(
        GetDungeonMapQuery request,
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

        return new GetDungeonMapResult
        {
            Seed = dungeon.Seed.ToString(),
            GeneratorVersion = dungeon.GeneratorVersion,
            Floor = floor.Index,
            FloorCount = dungeon.Settings.FloorCount,
            IsFinalFloor = floor.IsFinalFloor,
            Width = floor.Width,
            Height = floor.Height,
            Legend = DungeonContract.Legend,
            Rows = BuildRows(floor),
            Entrance = PositionResult.From(floor.Entrance),
            Rooms = floor.Rooms.Select(ToResult).ToList(),
            Elements = floor.Elements.Select(DungeonElementResult.From).ToList(),
        };
    }

    private static List<string> BuildRows(DungeonFloor floor)
    {
        List<string> rows = new(floor.Height);
        char[] row = new char[floor.Width];

        for (int y = 0; y < floor.Height; y++)
        {
            for (int x = 0; x < floor.Width; x++)
            {
                row[x] = DungeonContract.Symbol(floor.GetCell(new Position(x, y)));
            }

            rows.Add(new string(row));
        }

        return rows;
    }

    private static DungeonRoomResult ToResult(Room room) =>
        new(
            room.Id,
            DungeonContract.Name(room.Type),
            room.GridCell.X,
            room.GridCell.Y,
            room.Interior.X,
            room.Interior.Y,
            room.Interior.Width,
            room.Interior.Height,
            PositionResult.From(room.Center),
            room.Depth,
            room.ConnectedRoomIds
        );
}
