namespace Combat.Domain.Exceptions;

// Example of a known domain exception that the API middleware can map to 404.
// Keep one explicit exception per recurring business failure to avoid generic 500 responses.
public sealed class PlayerNotFoundException(Guid playerId)
    : KeyNotFoundException($"Player not found with PlayerId '{playerId}'.");
