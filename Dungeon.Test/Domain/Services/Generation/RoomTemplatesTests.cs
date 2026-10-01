using Dungeon.Domain.Enums;
using Dungeon.Domain.Services.Generation;
using FluentAssertions;

namespace Dungeon.Test.Domain.Services.Generation;

/// <summary>The hand-drawn rooms: every template must fit the generator's rules.</summary>
public class RoomTemplatesTests
{
    private const string WalkableTiles = ".etB$g";
    private const string KnownTiles = ".# oI=getB$~^T";

    public static TheoryData<string> TemplateNames()
    {
        TheoryData<string> data = [];
        foreach (RoomTemplate template in RoomTemplates.All)
        {
            data.Add(template.Name);
        }

        return data;
    }

    [Fact]
    public void All_HaveUniqueNames()
    {
        RoomTemplates.All.Select(template => template.Name).Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(RoomType.Start, 5)]
    [InlineData(RoomType.Combat, 25)]
    [InlineData(RoomType.Treasure, 7)]
    [InlineData(RoomType.Empty, 12)]
    [InlineData(RoomType.Boss, 8)]
    public void All_OfferSeveralTemplatesForEveryKindOfRoom(RoomType roomType, int minimum)
    {
        RoomTemplates
            .All.Count(template => template.RoomTypes.Contains(roomType))
            .Should()
            .BeGreaterThanOrEqualTo(minimum);
    }

    [Theory]
    [MemberData(nameof(TemplateNames))]
    public void Template_HasOddDimensionsAndOnlyKnownTiles(string name)
    {
        // Arrange
        RoomTemplate template = Find(name);

        // Assert: odd sizes centre the room exactly on its corridors.
        (template.Width % 2)
            .Should()
            .Be(1);
        (template.Height % 2).Should().Be(1);
        template.Rows.Should().OnlyContain(row => row.Length == template.Width);
        string.Concat(template.Rows).All(KnownTiles.Contains).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TemplateNames))]
    public void Template_CanBeEnteredFromEverySideAndCrossedToTheCentre(string name)
    {
        // Arrange
        RoomTemplate template = Find(name);
        int centreX = template.Width / 2;
        int centreY = template.Height / 2;
        (int X, int Y)[] entries =
        [
            (centreX, 0),
            (centreX, template.Height - 1),
            (0, centreY),
            (template.Width - 1, centreY),
        ];

        // Act
        HashSet<(int X, int Y)> reached = Reach(template, centreX, centreY);
        int walkable = string.Concat(template.Rows).Count(WalkableTiles.Contains);

        // Assert: a corridor may arrive in the middle of any side.
        IsWalkable(template, centreX, centreY).Should().BeTrue();
        entries.Should().OnlyContain(entry => reached.Contains(entry));
        reached.Should().HaveCount(walkable, "no walkable tile may be cut off");
    }

    [Theory]
    [MemberData(nameof(TemplateNames))]
    public void Template_HoldsTheSpotsItsKindOfRoomNeeds(string name)
    {
        // Arrange
        RoomTemplate template = Find(name);
        string tiles = string.Concat(template.Rows);
        int Count(char spot) => tiles.Count(tile => tile == spot);

        // Assert
        if (template.RoomTypes.Contains(RoomType.Start))
        {
            // The ladder the party climbs down stands in the middle, on plain floor.
            template
                .Rows[template.Height / 2][template.Width / 2]
                .Should()
                .Be(RoomTemplate.FloorTile);
        }

        if (template.RoomTypes.Contains(RoomType.Combat))
        {
            Count(RoomTemplate.EnemySpot).Should().BeGreaterThanOrEqualTo(2);
        }

        if (template.RoomTypes.Contains(RoomType.Treasure))
        {
            Count(RoomTemplate.TreasureSpot).Should().BeGreaterThanOrEqualTo(1);
        }

        if (template.RoomTypes.Contains(RoomType.Boss))
        {
            Count(RoomTemplate.BossSpot).Should().Be(1);

            // Columns frame the gate in the middle of the north wall.
            template.Rows[0][template.Width / 2].Should().Be(RoomTemplate.FloorTile);
        }

        Count(RoomTemplate.BossSpot)
            .Should()
            .Be(template.RoomTypes.Contains(RoomType.Boss) ? 1 : 0);
    }

    [Theory]
    [MemberData(nameof(TemplateNames))]
    public void Template_FitsTheLargestGridCell(string name)
    {
        RoomTemplate template = Find(name);

        template.Width.Should().BeInRange(7, 25);
        template.Height.Should().BeInRange(5, 19);
    }

    [Theory]
    [MemberData(nameof(TemplateNames))]
    public void Template_HasNoWallOneTileThick(string name)
    {
        // Arrange: a wall, or the void that becomes one, between two open tiles. Seen from
        // the front, such a wall reads as a flat slab.
        RoomTemplate template = Find(name);
        bool IsOpen(int x, int y) =>
            x >= 0
            && y >= 0
            && x < template.Width
            && y < template.Height
            && template.Rows[y][x] is not (RoomTemplate.WallTile or RoomTemplate.VoidTile);

        // Act
        IEnumerable<(int X, int Y)> thin =
            from y in Enumerable.Range(0, template.Height)
            from x in Enumerable.Range(0, template.Width)
            where template.Rows[y][x] is RoomTemplate.WallTile or RoomTemplate.VoidTile
            where (IsOpen(x - 1, y) && IsOpen(x + 1, y)) || (IsOpen(x, y - 1) && IsOpen(x, y + 1))
            select (x, y);

        // Assert
        thin.Should().BeEmpty();
    }

    [Fact]
    public void All_KeepSomeRoomsRare()
    {
        RoomTemplates.All.Should().Contain(template => template.Rarity == RoomRarity.Rare);
        RoomTemplates.All.Should().Contain(template => template.Rarity == RoomRarity.Uncommon);
        RoomTemplates
            .All.Count(template => template.Rarity == RoomRarity.Common)
            .Should()
            .BeGreaterThan(RoomTemplates.All.Count / 2);
    }

    [Fact]
    public void All_ComeInManySizesAndShapes()
    {
        RoomTemplates.All.Should().Contain(template => template.Width * template.Height <= 45);
        RoomTemplates.All.Should().Contain(template => template.Width * template.Height >= 400);
        // Shaped rooms: the void around their tiles.
        RoomTemplates
            .All.Count(template => template.Rows[0].Contains(RoomTemplate.VoidTile))
            .Should()
            .BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void At_Mirrored_ReadsTheRowRightToLeft()
    {
        RoomTemplate template = new("Test", [RoomType.Empty], ["o..", "...", "..I"]);

        template.At(0, 0, mirrored: false).Should().Be('o');
        template.At(2, 0, mirrored: true).Should().Be('o');
        template.At(0, 2, mirrored: true).Should().Be('I');
    }

    [Theory]
    [MemberData(nameof(TemplateNames))]
    public void Template_LaysItsTombsTwoOrThreeSideBySide(string name)
    {
        // Arrange: a tomb is drawn across the run of tiles it covers.
        RoomTemplate template = Find(name);

        foreach (string row in template.Rows)
        {
            // Act
            IEnumerable<int> runs = row.Split(
                    row.Where(tile => tile != RoomTemplate.TombTile).Distinct().ToArray(),
                    StringSplitOptions.RemoveEmptyEntries
                )
                .Select(run => run.Length);

            // Assert
            runs.Should().OnlyContain(length => length == 2 || length == 3);
        }
    }

    [Fact]
    public void All_HaveWaterLavaAndTombsSomewhere()
    {
        string tiles = string.Concat(RoomTemplates.All.SelectMany(template => template.Rows));

        tiles.Contains(RoomTemplate.WaterTile).Should().BeTrue();
        tiles.Contains(RoomTemplate.LavaTile).Should().BeTrue();
        tiles.Contains(RoomTemplate.TombTile).Should().BeTrue();
    }

    private static RoomTemplate Find(string name) =>
        RoomTemplates.All.Single(template => template.Name == name);

    private static bool IsWalkable(RoomTemplate template, int x, int y) =>
        x >= 0
        && y >= 0
        && x < template.Width
        && y < template.Height
        && WalkableTiles.Contains(template.Rows[y][x]);

    private static HashSet<(int X, int Y)> Reach(RoomTemplate template, int x, int y)
    {
        HashSet<(int X, int Y)> reached = [(x, y)];
        Queue<(int X, int Y)> queue = new([(x, y)]);
        while (queue.Count > 0)
        {
            (int cx, int cy) = queue.Dequeue();
            foreach ((int dx, int dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
            {
                if (IsWalkable(template, cx + dx, cy + dy) && reached.Add((cx + dx, cy + dy)))
                {
                    queue.Enqueue((cx + dx, cy + dy));
                }
            }
        }

        return reached;
    }
}
