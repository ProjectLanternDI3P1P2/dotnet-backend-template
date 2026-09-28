using Combat.Domain.Enums;
using Combat.Domain.ValueObjects;

namespace Combat.Domain.Services.Generation;

/// <summary>
/// Shared room helpers, and the rule every room layout must respect: after any change,
/// the centre and every door stay reachable, and no walkable tile of the room is cut off.
/// </summary>
internal static class RoomGeometry
{
    public static IEnumerable<Position> InteriorTiles(RoomDraft room)
    {
        RoomBounds interior = room.Interior;
        for (int y = interior.Y; y <= interior.Bottom; y++)
        {
            for (int x = interior.X; x <= interior.Right; x++)
            {
                yield return new Position(x, y);
            }
        }
    }

    /// <summary>The interior tile just inside each door of the room.</summary>
    public static List<Position> EntryTiles(RoomDraft room, IReadOnlyList<RoomDraft> rooms)
    {
        RoomBounds interior = room.Interior;
        List<Position> entries = [];

        foreach (int connectedId in room.Connections)
        {
            Position other = rooms[connectedId].GridCell;
            int dx = other.X - room.GridCell.X;
            int dy = other.Y - room.GridCell.Y;

            entries.Add(
                (dx, dy) switch
                {
                    (1, 0) => new Position(interior.Right, room.Center.Y),
                    (-1, 0) => new Position(interior.X, room.Center.Y),
                    (0, 1) => new Position(room.Center.X, interior.Bottom),
                    _ => new Position(room.Center.X, interior.Y),
                }
            );
        }

        return entries;
    }

    /// <summary>
    /// Sets <paramref name="tiles"/> to <paramref name="cellType"/>, keeping the change only
    /// if the room stays sound. Returns whether the change was kept.
    /// </summary>
    public static bool TryApply(
        TileGrid grid,
        RoomDraft room,
        IReadOnlyList<RoomDraft> rooms,
        IReadOnlyList<Position> tiles,
        CellType cellType
    )
    {
        if (
            tiles.Count == 0
            || tiles.Any(tile => !room.Interior.Contains(tile) || tile == room.Center)
        )
        {
            return false;
        }

        CellType[] previous = tiles.Select(tile => grid[tile.X, tile.Y]).ToArray();
        foreach (Position tile in tiles)
        {
            grid[tile.X, tile.Y] = cellType;
        }

        if (IsSound(grid, room, rooms))
        {
            return true;
        }

        for (int index = 0; index < tiles.Count; index++)
        {
            grid[tiles[index].X, tiles[index].Y] = previous[index];
        }

        return false;
    }

    public static bool IsSound(TileGrid grid, RoomDraft room, IReadOnlyList<RoomDraft> rooms)
    {
        RoomBounds interior = room.Interior;
        if (!grid[room.Center.X, room.Center.Y].IsWalkable())
        {
            return false;
        }

        bool[] visited = new bool[interior.Area];
        Queue<Position> queue = new();
        queue.Enqueue(room.Center);
        visited[Index(interior, room.Center)] = true;
        int reached = 0;

        while (queue.Count > 0)
        {
            Position current = queue.Dequeue();
            reached++;

            foreach (Direction direction in Enum.GetValues<Direction>())
            {
                Position next = current.Step(direction);
                if (
                    interior.Contains(next)
                    && !visited[Index(interior, next)]
                    && grid[next.X, next.Y].IsWalkable()
                )
                {
                    visited[Index(interior, next)] = true;
                    queue.Enqueue(next);
                }
            }
        }

        int walkable = InteriorTiles(room).Count(tile => grid[tile.X, tile.Y].IsWalkable());

        return reached == walkable
            && EntryTiles(room, rooms).All(entry => visited[Index(interior, entry)]);
    }

    private static int Index(RoomBounds interior, Position position)
    {
        return (position.Y - interior.Y) * interior.Width + (position.X - interior.X);
    }
}
