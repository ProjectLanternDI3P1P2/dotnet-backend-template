namespace Combat.Domain.Exceptions;

/// <summary>
/// The generator produced a dungeon that breaks a business rule. This is a bug, never an
/// expected outcome: it is thrown rather than letting an invalid dungeon reach a player.
/// </summary>
public sealed class DungeonGenerationException(string seed, IEnumerable<string> violations)
    : Exception(
        $"Dungeon generated from seed '{seed}' is invalid: {string.Join("; ", violations)}"
    );
