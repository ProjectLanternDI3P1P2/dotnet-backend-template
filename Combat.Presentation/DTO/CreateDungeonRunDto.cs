namespace Combat.Presentation.DTO;

public class CreateDungeonRunDto
{
    /// <summary>Optional idempotency key: Player sends it so that a retried call is harmless.</summary>
    public Guid? RunId { get; set; }

    public required Guid GameSessionId { get; set; }

    /// <summary>Omit it for a brand new dungeon; set it to replay a shared one.</summary>
    public string? Seed { get; set; }
}
