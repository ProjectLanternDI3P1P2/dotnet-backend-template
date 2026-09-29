namespace Dungeon.Domain.Exceptions;

/// <summary>
/// A business rule refused the operation in the current state. Presentation maps it to
/// HTTP 409, as opposed to input validation failures (422).
/// </summary>
public abstract class DomainException(string message) : Exception(message);
