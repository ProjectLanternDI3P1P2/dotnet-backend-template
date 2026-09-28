using Combat.Application.Abstractions;

namespace Combat.Application.Features.DungeonRunUseCase.CreateDungeonRun;

/// <param name="RunId">Chosen by the caller, so that a retried request is idempotent (ADR-0025).</param>
/// <param name="Seed">Null for a new dungeon; an existing seed to replay a shared adventure.</param>
public record CreateDungeonRunCommand(Guid RunId, Guid GameSessionId, string? Seed) : ICommand;
