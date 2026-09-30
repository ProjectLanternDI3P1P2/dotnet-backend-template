using Dungeon.Domain.Enums;

namespace Dungeon.Domain.Exceptions;

public sealed class DungeonRunNotActiveException(Guid runId, DungeonRunStatus status)
    : DomainException($"Dungeon run '{runId}' is {status} and no longer accepts actions.");
