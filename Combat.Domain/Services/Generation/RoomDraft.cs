using Combat.Domain.Enums;
using Combat.Domain.ValueObjects;

namespace Combat.Domain.Services.Generation;

/// <summary>Mutable room used while a floor is being built, frozen into a Room at the end.</summary>
internal sealed class RoomDraft(int id, Position gridCell)
{
    public int Id { get; } = id;

    public Position GridCell { get; } = gridCell;

    public List<int> Connections { get; } = [];

    public RoomType Type { get; set; } = RoomType.Combat;

    public RoomLayout Layout { get; set; } = RoomLayout.Plain;

    public int Depth { get; set; }

    public RoomBounds Interior { get; set; }

    public Position Center { get; set; }

    public bool IsDeadEnd => Connections.Count == 1;
}

/// <summary>How the inside of a room is arranged, on top of its size.</summary>
internal enum RoomLayout
{
    /// <summary>A few scattered barrels and jars.</summary>
    Plain,

    /// <summary>Corners cut away: L-shaped, T-shaped or cross-shaped rooms.</summary>
    Notched,

    /// <summary>Walled pits on the sides of the room.</summary>
    Pits,

    /// <summary>A wall splits off a side room, reached through a door.</summary>
    Partition,

    /// <summary>Rows of pillars.</summary>
    Colonnade,

    /// <summary>The treasure stands behind a fence with a single opening.</summary>
    Vault,

    /// <summary>Barrels and jars piled in the corners.</summary>
    Storage,
}
