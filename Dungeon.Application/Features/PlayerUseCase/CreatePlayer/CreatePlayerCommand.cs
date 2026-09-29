using Dungeon.Application.Abstractions;

namespace Dungeon.Application.Features.PlayerUseCase.CreatePlayer;

public record CreatePlayerCommand(Guid Id, string Name, int Attack, int Health, int MaxHealth)
    : ICommand;
