using Combat.Application.Abstractions;

namespace Combat.Application.Features.DungeonRunUseCase.MoveHero;

/// <param name="Direction">north, east, south or west: one tile per command (turn-based).</param>
public record MoveHeroCommand(Guid RunId, string Direction) : ICommand;
