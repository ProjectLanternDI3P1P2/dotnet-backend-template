using Combat.Application.Models;

namespace Combat.Application.Features.DungeonUseCase.GetDungeonMap;

/// <summary>
/// A whole floor. Cells are sent as one text row per line (<see cref="Rows"/>, decoded with
/// <see cref="Legend"/>) and elements as a sparse list: a 40-room floor weighs about 20 KB
/// instead of about 1 MB as one JSON object per cell, and compresses to a few KB.
/// </summary>
public sealed class GetDungeonMapResult
{
    public string Seed { get; init; } = string.Empty;
    public int GeneratorVersion { get; init; }
    public int Floor { get; init; }
    public int FloorCount { get; init; }
    public bool IsFinalFloor { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public IReadOnlyDictionary<string, string> Legend { get; init; } =
        new Dictionary<string, string>();
    public IReadOnlyList<string> Rows { get; init; } = [];
    public PositionResult Entrance { get; init; } = new(0, 0);
    public IReadOnlyList<DungeonRoomResult> Rooms { get; init; } = [];
    public IReadOnlyList<DungeonElementResult> Elements { get; init; } = [];
}

public sealed record DungeonRoomResult(
    int Id,
    string Type,
    int GridX,
    int GridY,
    int X,
    int Y,
    int Width,
    int Height,
    PositionResult Center,
    int Depth,
    IReadOnlyList<int> ConnectedRoomIds
);
