using Dungeon.Domain.Enums;
using Dungeon.Domain.ValueObjects;

namespace Dungeon.Domain.Entities;

/// <summary>One generated floor: its tile grid, rooms and elements. Immutable.</summary>
public sealed class DungeonFloor
{
    private const short NoRoom = -1;

    private readonly CellType[] _cells;
    private readonly short[] _roomIdByCell;
    private readonly Dictionary<Position, DungeonElement[]> _elementsByPosition;

    public DungeonFloor(
        int index,
        bool isFinalFloor,
        int width,
        int height,
        CellType[] cells,
        IReadOnlyList<Room> rooms,
        IReadOnlyList<DungeonElement> elements,
        Position entrance
    )
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(cells.Length, width * height);

        Index = index;
        IsFinalFloor = isFinalFloor;
        Width = width;
        Height = height;
        _cells = cells;
        Rooms = rooms;
        Elements = elements;
        Entrance = entrance;

        _roomIdByCell = new short[cells.Length];
        Array.Fill(_roomIdByCell, NoRoom);
        foreach (Room room in rooms)
        {
            RoomBounds interior = room.Interior;
            for (int y = interior.Y; y <= interior.Bottom; y++)
            {
                for (int x = interior.X; x <= interior.Right; x++)
                {
                    // Pits and partition walls inside the bounds do not belong to the room.
                    CellType cell = cells[y * width + x];
                    if (cell is not (CellType.Wall or CellType.Void))
                    {
                        _roomIdByCell[y * width + x] = (short)room.Id;
                    }
                }
            }
        }

        _elementsByPosition = elements
            .GroupBy(element => element.Position)
            .ToDictionary(group => group.Key, group => group.ToArray());
    }

    public int Index { get; }

    public bool IsFinalFloor { get; }

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyList<Room> Rooms { get; }

    public IReadOnlyList<DungeonElement> Elements { get; }

    /// <summary>
    /// Where the party stands when it arrives on this floor: at the foot of the ladder it
    /// climbs down, in the middle of the start room.
    /// </summary>
    public Position Entrance { get; }

    public bool Contains(Position position)
    {
        return position.X >= 0 && position.Y >= 0 && position.X < Width && position.Y < Height;
    }

    public CellType GetCell(Position position)
    {
        EnsureContains(position);
        return _cells[position.Y * Width + position.X];
    }

    public bool IsWalkable(Position position)
    {
        return Contains(position) && GetCell(position).IsWalkable();
    }

    /// <summary>The room the position belongs to; null in corridors, outer doors, walls and pits.</summary>
    public int? GetRoomId(Position position)
    {
        EnsureContains(position);
        int roomId = _roomIdByCell[position.Y * Width + position.X];
        return roomId == NoRoom ? null : roomId;
    }

    public IReadOnlyList<DungeonElement> GetElementsAt(Position position)
    {
        return _elementsByPosition.TryGetValue(position, out DungeonElement[]? elements)
            ? elements
            : [];
    }

    private void EnsureContains(Position position)
    {
        if (!Contains(position))
        {
            throw new ArgumentOutOfRangeException(
                nameof(position),
                position,
                $"Position is outside the {Width}x{Height} floor."
            );
        }
    }
}
