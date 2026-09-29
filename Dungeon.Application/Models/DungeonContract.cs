using Dungeon.Domain.Entities;
using Dungeon.Domain.Enums;
using Dungeon.Domain.ValueObjects;

namespace Dungeon.Application.Models;

public sealed record PositionResult(int X, int Y)
{
    public static PositionResult From(Position position) => new(position.X, position.Y);
}

public sealed record DungeonElementResult(int Id, string Type, int X, int Y, int RoomId)
{
    public static DungeonElementResult From(DungeonElement element) =>
        new(
            element.Id,
            DungeonContract.Name(element.Type),
            element.Position.X,
            element.Position.Y,
            element.RoomId
        );
}

/// <summary>
/// Names used on the wire for the dungeon enums, written out by hand (ADR-0033) so that
/// renaming a C# member never breaks the Gateway, the frontend or another service.
/// </summary>
public static class DungeonContract
{
    /// <summary>One character per cell type, used to send a floor as rows of text.</summary>
    public static char Symbol(CellType cellType) =>
        cellType switch
        {
            CellType.Void => ' ',
            CellType.Floor => '.',
            CellType.Wall => '#',
            CellType.Door => '+',
            CellType.Obstacle => 'o',
            CellType.StairsDown => '>',
            CellType.StairsUp => '<',
            CellType.Pillar => 'I',
            CellType.Fence => '=',
            CellType.Gate => 'G',
            CellType.Grate => 'g',
            _ => throw new ArgumentOutOfRangeException(nameof(cellType), cellType, null),
        };

    public static string Name(CellType cellType) =>
        cellType switch
        {
            CellType.Void => "void",
            CellType.Floor => "floor",
            CellType.Wall => "wall",
            CellType.Door => "door",
            CellType.Obstacle => "obstacle",
            CellType.StairsDown => "stairsDown",
            CellType.StairsUp => "stairsUp",
            CellType.Pillar => "pillar",
            CellType.Fence => "fence",
            CellType.Gate => "gate",
            CellType.Grate => "grate",
            _ => throw new ArgumentOutOfRangeException(nameof(cellType), cellType, null),
        };

    public static string Name(ElementType elementType) =>
        elementType switch
        {
            ElementType.Enemy => "enemy",
            ElementType.Boss => "boss",
            ElementType.Item => "item",
            ElementType.Trap => "trap",
            _ => throw new ArgumentOutOfRangeException(nameof(elementType), elementType, null),
        };

    public static string Name(RoomType roomType) =>
        roomType switch
        {
            RoomType.Start => "start",
            RoomType.Combat => "combat",
            RoomType.Treasure => "treasure",
            RoomType.Empty => "empty",
            RoomType.Boss => "boss",
            RoomType.Stairs => "stairs",
            _ => throw new ArgumentOutOfRangeException(nameof(roomType), roomType, null),
        };

    public static string Name(DungeonRunStatus status) =>
        status switch
        {
            DungeonRunStatus.Active => "active",
            DungeonRunStatus.Won => "won",
            DungeonRunStatus.Lost => "lost",
            DungeonRunStatus.Abandoned => "abandoned",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };

    /// <summary>Symbol to cell type name, sent with every map so that clients never hard-code it.</summary>
    public static IReadOnlyDictionary<string, string> Legend { get; } =
        Enum.GetValues<CellType>().ToDictionary(type => Symbol(type).ToString(), Name);

    /// <summary>Parses a direction sent by a client: north, east, south or west, any case.</summary>
    public static bool TryParseDirection(string? value, out Direction direction)
    {
        (bool parsed, direction) = value?.ToLowerInvariant() switch
        {
            "north" => (true, Direction.North),
            "east" => (true, Direction.East),
            "south" => (true, Direction.South),
            "west" => (true, Direction.West),
            _ => (false, default),
        };

        return parsed;
    }
}
