using Combat.Domain.Enums;
using Combat.Domain.ValueObjects;

namespace Combat.Domain.Services.Generation;

/// <summary>Mutable room used while a floor is being built, frozen into a Room at the end.</summary>
internal sealed class RoomDraft(int id, Position gridCell)
{
    public int Id { get; } = id;

    /// <summary>Settable: the whole floor may be flipped so that the stairs room lies north.</summary>
    public Position GridCell { get; set; } = gridCell;

    public List<int> Connections { get; } = [];

    public RoomType Type { get; set; } = RoomType.Combat;

    public int Depth { get; set; }

    public RoomTemplate Template { get; set; } = null!;

    public bool IsMirrored { get; set; }

    public RoomBounds Interior { get; set; }

    public Position Center { get; set; }

    /// <summary>The spots drawn in the template, in reading order, in floor coordinates.</summary>
    public Dictionary<char, List<Position>> Spots { get; } = [];

    public bool IsDeadEnd => Connections.Count == 1;

    public List<Position> SpotsOf(char spot)
    {
        return Spots.TryGetValue(spot, out List<Position>? positions) ? positions : [];
    }
}
