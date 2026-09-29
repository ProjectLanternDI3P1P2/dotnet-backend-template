using Dungeon.Domain.Enums;
using Dungeon.Domain.Exceptions;
using Dungeon.Domain.ValueObjects;

namespace Dungeon.Domain.Entities;

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

    /// <summary>
    /// Whether the boss of the current floor is defeated, which opens the gate to the stairs
    /// room. Reset on arrival at a new floor.
    /// </summary>
    public bool IsFloorBossDefeated { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public Position HeroPosition => new(HeroX, HeroY);

    public DungeonSettings Settings => new(RoomCount, FloorCount);

    public static DungeonRun Start(
        Guid runId,
        Guid gameSessionId,
        GeneratedDungeon dungeon,
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
    /// Moves the hero exactly one tile. Walls, obstacles, the void, anything outside the floor
    /// and the gate of a boss still standing are rejected; the run is left unchanged when a
    /// move is refused.
    /// </summary>
    public Position MoveHero(Direction direction, GeneratedDungeon dungeon)
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

        if (cell == CellType.Gate && !IsFloorBossDefeated)
        {
            throw new InvalidMoveException(
                target,
                "the gate stays locked until the boss of this floor is defeated"
            );
        }

        HeroX = target.X;
        HeroY = target.Y;
        Turn++;

        return target;
    }

    /// <summary>Takes the stairs the hero stands on and arrives at the next floor's entrance.</summary>
    public int TakeStairsDown(GeneratedDungeon dungeon)
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
        IsFloorBossDefeated = false;
        Turn++;

        return CurrentFloor;
    }

    /// <summary>
    /// Records the victory over the boss of the current floor: the gate to the stairs opens,
    /// and defeating the boss of the last floor wins the run. The fight itself belongs to
    /// Combat; the hero must be in the boss room. Recording it twice changes nothing.
    /// </summary>
    public void DefeatFloorBoss(GeneratedDungeon dungeon)
    {
        DungeonFloor floor = GetCurrentFloor(dungeon);
        if (IsFloorBossDefeated)
        {
            return;
        }

        Room bossRoom = floor.Rooms.Single(room => room.Type == RoomType.Boss);
        if (floor.GetRoomId(HeroPosition) != bossRoom.Id)
        {
            throw new BossNotInReachException(Id, CurrentFloor);
        }

        IsFloorBossDefeated = true;
        Turn++;

        if (floor.IsFinalFloor)
        {
            Status = DungeonRunStatus.Won;
        }
    }

    private DungeonFloor GetCurrentFloor(GeneratedDungeon dungeon)
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
