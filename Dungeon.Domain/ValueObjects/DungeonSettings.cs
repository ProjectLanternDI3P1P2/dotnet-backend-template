namespace Dungeon.Domain.ValueObjects;

/// <summary>
/// The shape of a dungeon: how many rooms it holds in total and across how many floors.
/// A run stores its settings next to its seed, because the same seed with other
/// settings is another dungeon.
/// </summary>
public sealed record DungeonSettings
{
    /// <summary>
    /// Business rule of US-DUNGEON-01: a dungeon contains exactly 40 rooms. The stairs rooms,
    /// which only hold the way down, are not counted.
    /// </summary>
    public const int DefaultRoomCount = 40;

    /// <summary>Four floors of ten rooms, each ending with a boss.</summary>
    public const int DefaultFloorCount = 4;

    public const int MinimumRoomsPerFloor = 8;
    public const int MaximumRoomsPerFloor = 80;

    public DungeonSettings(int roomCount, int floorCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(floorCount, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(roomCount, MinimumRoomsPerFloor * floorCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            roomCount,
            MaximumRoomsPerFloor * floorCount
        );

        RoomCount = roomCount;
        FloorCount = floorCount;
    }

    public static DungeonSettings Default { get; } = new(DefaultRoomCount, DefaultFloorCount);

    public int RoomCount { get; }

    public int FloorCount { get; }

    /// <summary>
    /// Splits the rooms as evenly as possible, the first floors taking the remainder:
    /// 40 rooms on 4 floors gives 10 each, 40 rooms on 3 floors gives 14, 13 and 13.
    /// </summary>
    public int RoomCountForFloor(int floorIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(floorIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(floorIndex, FloorCount);

        return RoomCount / FloorCount + (floorIndex < RoomCount % FloorCount ? 1 : 0);
    }
}
