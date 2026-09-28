using Combat.Application.Models;

namespace Combat.Application.Features.DungeonUseCase.GetDungeonCell;

public sealed class GetDungeonCellResult
{
    public string Seed { get; init; } = string.Empty;
    public int Floor { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public string Type { get; init; } = string.Empty;
    public bool IsWalkable { get; init; }

    /// <summary>The room whose interior holds the cell; null in corridors, doors and walls.</summary>
    public int? RoomId { get; init; }

    public IReadOnlyList<DungeonElementResult> Elements { get; init; } = [];
}
