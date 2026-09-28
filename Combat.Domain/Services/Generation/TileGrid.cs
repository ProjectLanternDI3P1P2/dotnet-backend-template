using Combat.Domain.Enums;

namespace Combat.Domain.Services.Generation;

/// <summary>The mutable tile grid of a floor while it is being generated.</summary>
internal sealed class TileGrid(int width, int height)
{
    public int Width { get; } = width;

    public int Height { get; } = height;

    public CellType[] Cells { get; } = new CellType[width * height];

    public CellType this[int x, int y]
    {
        get => Cells[y * Width + x];
        set => Cells[y * Width + x] = value;
    }

    public bool Contains(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }
}
