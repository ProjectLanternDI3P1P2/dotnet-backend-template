using Combat.Domain.Enums;
using Combat.Domain.ValueObjects;

namespace Combat.Domain.Entities;

/// <summary>
/// An abstract element placed by the generator. <see cref="Id"/> is stable for a given
/// seed and floor, so other services can reference it.
/// </summary>
public sealed record DungeonElement(int Id, ElementType Type, Position Position, int RoomId);
