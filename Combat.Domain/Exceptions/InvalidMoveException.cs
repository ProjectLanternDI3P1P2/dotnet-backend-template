using Combat.Domain.ValueObjects;

namespace Combat.Domain.Exceptions;

public sealed class InvalidMoveException(Position target, string reason)
    : DomainException($"The hero cannot move to ({target.X}, {target.Y}): {reason}.")
{
    public Position Target { get; } = target;
}
