---
name: backend-naming
description: Apply shared backend naming conventions when creating or renaming code, contracts, messages, identifiers, or tests.
---

# Backend Naming

Always follow `BACKEND_NAMING_CONVENTIONS.md`.

## C# names

Use:

- PascalCase for namespaces, classes, records, methods, properties, and constants;
- `I` + PascalCase for interfaces;
- `_camelCase` for private fields;
- camelCase for parameters and local variables.

Treat abbreviations as normal words:

```text
ApiClient
HttpClient
JsonSerializer
UserId
```

Do not use:

```text
APIClient
HTTPClient
UserID
```

## Identifiers

Use `<Concept>Id`.

```text
PlayerId
DungeonId
CombatId
RewardId
MessageId
CorrelationId
CausationId
```

Do not use `IdPlayer`, `PlayerID`, or `IDPlayer`.

Use `Guid` unless an existing documented exception applies.

## Dates

Use `DateTimeOffset`.

Use consistent names:

```text
CreatedAt
UpdatedAt
DeletedAt
StartedAt
CompletedAt
OccurredAt
ExpiresAt
PublishedAt
```

## Booleans

Prefer:

```text
IsActive
IsCompleted
HasStarted
HasExpired
CanRetry
ShouldPublish
```

## Collections

Use plural collection names.

## Async methods

All asynchronous methods end with `Async`.

Reuse established verbs:

```text
GetByIdAsync
GetAllAsync
CreateAsync
UpdateAsync
DeleteAsync
ExistsAsync
```

Use domain verbs when they add meaning.

## CQRS

Use:

```text
<CreateThing>Command
<CreateThing>CommandHandler
<CreateThing>Validator
<GetThing>Query
<GetThing>QueryHandler
```

## DTOs

Use explicit names such as:

```text
CreatePlayerRequest
PlayerDto
CombatDto
```

Do not create vague names such as `DataDto`, `ModelDto`, or `ResponseDto`.

## EF Core

Use `<Entity>Configuration`.

## Messages

Events use explicit past-tense business names.

Use versioned logical destinations:

```text
<domain>.<message>.v<version>
```

## Tests

Use:

```text
Method_Scenario_ExpectedResult
```

Reuse existing project vocabulary. Do not create synonyms for an existing concept.
