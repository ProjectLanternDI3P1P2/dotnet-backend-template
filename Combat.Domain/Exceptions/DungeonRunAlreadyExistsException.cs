namespace Combat.Domain.Exceptions;

public sealed class DungeonRunAlreadyExistsException(Guid runId)
    : DomainException($"Dungeon run '{runId}' already exists for another game session.");
