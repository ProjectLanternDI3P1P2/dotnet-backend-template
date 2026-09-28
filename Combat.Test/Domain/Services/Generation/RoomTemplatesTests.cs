using Combat.Domain.Enums;
using Combat.Domain.Services.Generation;
using FluentAssertions;

namespace Combat.Test.Domain.Services.Generation;

/// <summary>The hand-drawn rooms: every template must fit the generator's rules.</summary>
public class RoomTemplatesTests
{
    private const string WalkableTiles = ".etB$g<>";
    private const string KnownTiles = ".# oI=getB$<>";

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
    [InlineData(RoomType.Start, 3)]
    [InlineData(RoomType.Combat, 10)]
    [InlineData(RoomType.Treasure, 3)]
    [InlineData(RoomType.Empty, 3)]
    [InlineData(RoomType.Boss, 3)]
    [InlineData(RoomType.Stairs, 3)]
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
        (template.Width % 2).Should().Be(1);
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
            Count(RoomTemplate.ArrivalSpot).Should().Be(1);
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

        if (template.RoomTypes.Contains(RoomType.Stairs))
        {
            Count(RoomTemplate.StairsDownTile).Should().BeGreaterThanOrEqualTo(1);
        }

        Count(RoomTemplate.BossSpot).Should().Be(template.RoomTypes.Contains(RoomType.Boss) ? 1 : 0);
        Count(RoomTemplate.ArrivalSpot).Should().Be(template.RoomTypes.Contains(RoomType.Start) ? 1 : 0);
    }

    [Theory]
    [MemberData(nameof(TemplateNames))]
    public void Template_FitsTheLargestGridCell(string name)
    {
        RoomTemplate template = Find(name);

        template.Width.Should().BeInRange(7, 21);
        template.Height.Should().BeInRange(5, 13);
    }

    [Fact]
    public void At_Mirrored_ReadsTheRowRightToLeft()
    {
        RoomTemplate template = new("Test", [RoomType.Empty], ["o..", "...", "..I"]);

        template.At(0, 0, mirrored: false).Should().Be('o');
        template.At(2, 0, mirrored: true).Should().Be('o');
        template.At(0, 2, mirrored: true).Should().Be('I');
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
