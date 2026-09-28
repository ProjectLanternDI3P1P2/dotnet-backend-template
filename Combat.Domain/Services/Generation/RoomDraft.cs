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

    public int Depth { get; set; }

    public RoomBounds Interior { get; set; }

    public Position Center { get; set; }

    public bool IsDeadEnd => Connections.Count == 1;
}
