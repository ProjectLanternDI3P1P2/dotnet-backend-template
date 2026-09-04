namespace Combat.Infrastructure.Services;

// Example of an infrastructure service. Keep technical adapters here so
// Application and Domain code can depend on abstractions instead of system APIs.
public sealed class SystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
