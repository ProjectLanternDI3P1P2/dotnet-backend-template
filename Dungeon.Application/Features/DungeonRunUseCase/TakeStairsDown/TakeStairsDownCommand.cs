using Dungeon.Application.Abstractions;

namespace Dungeon.Application.Features.DungeonRunUseCase.TakeStairsDown;

/// <summary>The hero must stand on the stairs down; the party arrives on the next floor.</summary>
public record TakeStairsDownCommand(Guid RunId) : ICommand;
