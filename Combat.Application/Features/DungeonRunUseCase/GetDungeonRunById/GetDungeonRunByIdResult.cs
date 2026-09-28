using Combat.Application.Models;

namespace Combat.Application.Features.DungeonRunUseCase.GetDungeonRunById;

public sealed class GetDungeonRunByIdResult
{
    public Guid Id { get; init; }
    public Guid GameSessionId { get; init; }
    public string Seed { get; init; } = string.Empty;
    public int GeneratorVersion { get; init; }
    public string Status { get; init; } = string.Empty;
    public int FloorCount { get; init; }
    public int CurrentFloor { get; init; }
    public PositionResult Hero { get; init; } = new(0, 0);
    public int Turn { get; init; }

    /// <summary>The boss of the current floor is defeated: the gate to the stairs is open.</summary>
    public bool FloorBossDefeated { get; init; }

    /// <summary>The room the hero stands in; null in a corridor or a doorway.</summary>
    public int? CurrentRoomId { get; init; }

    /// <summary>Elements on the hero's tile: an enemy there is where Combat takes over.</summary>
    public IReadOnlyList<DungeonElementResult> ElementsHere { get; init; } = [];

    public DateTimeOffset StartedAt { get; init; }
}
