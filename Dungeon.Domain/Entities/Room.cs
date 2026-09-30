using Dungeon.Domain.Enums;
using Dungeon.Domain.ValueObjects;

namespace Dungeon.Domain.Entities;

public sealed class Room
{
    public Room(
        int id,
        RoomType type,
        Position gridCell,
        RoomBounds interior,
        Position center,
        int depth,
        IReadOnlyList<int> connectedRoomIds
    )
    {
        Id = id;
        Type = type;
        GridCell = gridCell;
        Interior = interior;
        Center = center;
        Depth = depth;
        ConnectedRoomIds = connectedRoomIds;
    }

    /// <summary>Unique on its floor. The start room is always 0.</summary>
    public int Id { get; }

    public RoomType Type { get; }

    /// <summary>Coordinates on the coarse room grid, used for the minimap.</summary>
    public Position GridCell { get; }

    public RoomBounds Interior { get; }

    /// <summary>The tile every corridor of the room lines up with. Never blocked.</summary>
    public Position Center { get; }

    /// <summary>Number of rooms to cross from the start room. Drives difficulty.</summary>
    public int Depth { get; }

    public IReadOnlyList<int> ConnectedRoomIds { get; }
}
