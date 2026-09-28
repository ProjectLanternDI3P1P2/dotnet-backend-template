namespace Combat.Domain.Exceptions;

/// <summary>The boss of a floor can only be fought from inside its room.</summary>
public sealed class BossNotInReachException(Guid runId, int floor)
    : DomainException(
        $"The hero of dungeon run '{runId}' is not in the boss room of floor {floor}."
    );
