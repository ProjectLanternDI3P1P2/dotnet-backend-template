using Combat.Domain.Entities;
using Combat.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Combat.Infrastructure.Persistence.Configurations;

public class DungeonRunConfiguration : IEntityTypeConfiguration<DungeonRun>
{
    public void Configure(EntityTypeBuilder<DungeonRun> builder)
    {
        builder.ToTable("DungeonRuns");

        builder.HasKey(run => run.Id);

        builder.Property(run => run.Id).IsRequired().HasColumnName("DungeonRunId");

        builder.Property(run => run.GameSessionId).IsRequired();

        // Stored as its shareable text form, so that a seed read in the database is the one
        // a player types.
        builder
            .Property(run => run.Seed)
            .IsRequired()
            .HasMaxLength(Seed.TextLength)
            .HasConversion(seed => seed.ToString(), text => Seed.Parse(text));

        builder.Property(run => run.GeneratorVersion).IsRequired();

        builder.Property(run => run.RoomCount).IsRequired();

        builder.Property(run => run.FloorCount).IsRequired();

        builder.Property(run => run.Status).IsRequired().HasMaxLength(20).HasConversion<string>();

        builder.Property(run => run.CurrentFloor).IsRequired();

        builder.Property(run => run.HeroX).IsRequired();

        builder.Property(run => run.HeroY).IsRequired();

        // Two moves sent at the same time for the same run: the second one fails instead of
        // silently overwriting the first.
        builder.Property(run => run.Turn).IsRequired().IsConcurrencyToken();

        builder.Property(run => run.StartedAt).IsRequired();

        builder.Ignore(run => run.HeroPosition);

        builder.Ignore(run => run.Settings);

        builder.HasIndex(run => run.Seed);

        builder.HasIndex(run => run.GameSessionId);
    }
}
