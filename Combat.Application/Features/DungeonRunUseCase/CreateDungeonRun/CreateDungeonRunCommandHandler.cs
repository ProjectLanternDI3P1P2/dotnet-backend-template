using Combat.Application.Ports;
using Combat.Domain.Entities;
using Combat.Domain.Exceptions;
using Combat.Domain.Repositories;
using Combat.Domain.Services;
using Combat.Domain.Services.Generation;
using Combat.Domain.ValueObjects;
using MediatR;

namespace Combat.Application.Features.DungeonRunUseCase.CreateDungeonRun;

public sealed class CreateDungeonRunCommandHandler(
    IDungeonRunRepository dungeonRunRepository,
    IDungeonProvider dungeonProvider,
    ISeedGenerator seedGenerator,
    DungeonSettings dungeonSettings,
    IClock clock
) : IRequestHandler<CreateDungeonRunCommand>
{
    private const int MaximumSeedDraws = 5;

    public async Task Handle(CreateDungeonRunCommand request, CancellationToken cancellationToken)
    {
        DungeonRun? existing = await dungeonRunRepository.GetByIdAsync(
            request.RunId,
            cancellationToken
        );
        if (existing is not null)
        {
            if (existing.GameSessionId != request.GameSessionId)
            {
                throw new DungeonRunAlreadyExistsException(request.RunId);
            }

            // Player calls this synchronously and may retry: the same request is a no-op.
            return;
        }

        (Seed seed, DungeonSettings settings, int generatorVersion) = request.Seed is null
            ? (
                await DrawUnusedSeedAsync(cancellationToken),
                dungeonSettings,
                DungeonGenerator.CurrentVersion
            )
            : await GetReplaySetupAsync(Seed.Parse(request.Seed), cancellationToken);

        // Generating here also proves the dungeon is valid before the run is stored.
        Dungeon dungeon = dungeonProvider.Get(seed, settings, generatorVersion);

        await dungeonRunRepository.AddAsync(
            DungeonRun.Start(request.RunId, request.GameSessionId, dungeon, clock.UtcNow),
            cancellationToken
        );
    }

    /// <summary>
    /// Every new exploration gets a seed no run has used (US-DUNGEON-01). With 64 random bits a
    /// collision is already negligible; checking makes the rule hold by construction.
    /// </summary>
    private async Task<Seed> DrawUnusedSeedAsync(CancellationToken cancellationToken)
    {
        for (int draw = 0; draw < MaximumSeedDraws; draw++)
        {
            Seed candidate = seedGenerator.NewSeed();
            if (await dungeonRunRepository.FindLatestBySeedAsync(candidate, cancellationToken) is null)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException(
            $"No unused seed found after {MaximumSeedDraws} draws: the seed generator is broken."
        );
    }

    /// <summary>A replayed seed reuses the settings and version of its first run, so it is the same dungeon.</summary>
    private async Task<(Seed, DungeonSettings, int)> GetReplaySetupAsync(
        Seed seed,
        CancellationToken cancellationToken
    )
    {
        DungeonRun? original = await dungeonRunRepository.FindLatestBySeedAsync(
            seed,
            cancellationToken
        );

        return original is null
            ? (seed, dungeonSettings, DungeonGenerator.CurrentVersion)
            : (seed, original.Settings, original.GeneratorVersion);
    }
}
