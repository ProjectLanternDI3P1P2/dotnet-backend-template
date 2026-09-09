---
name: backend-observability
description: Apply structured logging, OpenTelemetry tracing, correlation context, exception logging, and cancellation propagation.
---

# Backend Observability

## Logging

Use structured logging.

Prefer:

```csharp
logger.LogInformation(
    "Player {PlayerId} started dungeon {DungeonId}",
    playerId,
    dungeonId);
```

Do not use string interpolation for structured values.

For exceptions:

```csharp
logger.LogError(
    exception,
    "Failed to process combat {CombatId}",
    combatId);
```

Do not log:

- passwords;
- secrets;
- tokens;
- sensitive authentication data.

Use useful business identifiers when they improve diagnosis.

## OpenTelemetry

Preserve tracing context across service boundaries.

Add spans for important external operations when consistent with the existing codebase.

Preserve correlation identifiers when they exist.

Useful context may include:

```text
PlayerId
DungeonId
CombatId
RewardId
MessageId
CorrelationId
```

Do not break existing trace or correlation propagation.

## Unexpected errors

Unexpected exceptions must be logged.

Do not expose internal exception details to API clients in production.

## Cancellation

Propagate `CancellationToken` through asynchronous operations when meaningful.

Controllers pass cancellation to MediatR.

Application passes cancellation to infrastructure abstractions.

Infrastructure passes cancellation to supported external and persistence APIs.
