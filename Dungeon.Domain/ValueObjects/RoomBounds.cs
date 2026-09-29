namespace Dungeon.Domain.ValueObjects;

/// <summary>The walkable interior of a room, walls excluded.</summary>
public readonly record struct RoomBounds(int X, int Y, int Width, int Height)
{
    public int Right => X + Width - 1;

    public int Bottom => Y + Height - 1;

    public int Area => Width * Height;

    public bool Contains(Position position)
    {
        return position.X >= X
            && position.X <= Right
            && position.Y >= Y
            && position.Y <= Bottom;
    }
}
