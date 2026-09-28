using Combat.Domain.Enums;
using Combat.Domain.Exceptions;
using Combat.Domain.ValueObjects;

namespace Combat.Domain.Entities;

/// <summary>
/// One exploration of a generated dungeon by a Player game session (ADR-GLOB-011).
/// Only what changes during play is stored; the dungeon itself is regenerated from the
/// seed, the settings and the generator version.
/// </summary>
public sealed class DungeonRun
{
    // Required by EF Core.
    private DungeonRun() { }

    public Guid Id { get; private set; }

    public Guid GameSessionId { get; private set; }

    public Seed Seed { get; private set; }

    public int GeneratorVersion { get; private set; }

    public int RoomCount { get; private set; }

    public int FloorCount { get; private set; }

    public DungeonRunStatus Status { get; private set; }

    public int CurrentFloor { get; private set; }

    public int HeroX { get; private set; }

    public int HeroY { get; private set; }

    /// <summary>Incremented by every accepted action: the game is turn-based.</summary>
    public int Turn { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public Position HeroPosition => new(HeroX, HeroY);

    public DungeonSettings Settings => new(RoomCount, FloorCount);

    public static DungeonRun Start(
        Guid runId,
        Guid gameSessionId,
        Dungeon dungeon,
        DateTimeOffset startedAt
    )
    {
        Position entrance = dungeon.Floors[0].Entrance;

        return new DungeonRun
        {
            Id = runId,
            GameSessionId = gameSessionId,
            Seed = dungeon.Seed,
            GeneratorVersion = dungeon.GeneratorVersion,
            RoomCount = dungeon.Settings.RoomCount,
            FloorCount = dungeon.Settings.FloorCount,
            Status = DungeonRunStatus.Active,
            CurrentFloor = 0,
            HeroX = entrance.X,
            HeroY = entrance.Y,
            Turn = 0,
            StartedAt = startedAt,
        };
    }

    /// <summary>
    /// Moves the hero exactly one tile. Walls, obstacles, the void and anything outside the
    /// floor are rejected; the run is left unchanged when a move is refused.
    /// </summary>
    public Position MoveHero(Direction direction, Dungeon dungeon)
    {
        DungeonFloor floor = GetCurrentFloor(dungeon);
        Position target = HeroPosition.Step(direction);

        if (!floor.Contains(target))
        {
            throw new InvalidMoveException(target, "the tile is outside the dungeon");
        }

        CellType cell = floor.GetCell(target);
        if (!cell.IsWalkable())
        {
            throw new InvalidMoveException(target, $"a {cell} tile is not walkable");
        }

        HeroX = target.X;
        HeroY = target.Y;
        Turn++;

        return target;
    }

    /// <summary>Takes the stairs the hero stands on and arrives at the next floor's entrance.</summary>
    public int TakeStairsDown(Dungeon dungeon)
    {
        DungeonFloor floor = GetCurrentFloor(dungeon);

        if (floor.GetCell(HeroPosition) != CellType.StairsDown)
        {
            throw new InvalidMoveException(HeroPosition, "there are no stairs down here");
        }

        CurrentFloor++;
        Position entrance = dungeon.Floors[CurrentFloor].Entrance;
        HeroX = entrance.X;
        HeroY = entrance.Y;
        Turn++;

        return CurrentFloor;
    }

    private DungeonFloor GetCurrentFloor(Dungeon dungeon)
    {
        if (Status != DungeonRunStatus.Active)
        {
            throw new DungeonRunNotActiveException(Id, Status);
        }

        if (dungeon.Seed != Seed || dungeon.GeneratorVersion != GeneratorVersion)
        {
            throw new ArgumentException(
                $"Dungeon {dungeon.Seed} v{dungeon.GeneratorVersion} is not the dungeon of run '{Id}'.",
                nameof(dungeon)
            );
        }

        return dungeon.Floors[CurrentFloor];
    }
}
