using Serilog.Core;
using Serilog.Events;

namespace Combat.Presentation.Extensions.LogExtension;

public sealed class LowercaseLevelEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var shortLevel = logEvent.Level.ToShortString();
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("level", shortLevel));
    }
}
