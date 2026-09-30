# Backend Naming Conventions

> Shared naming conventions for all backend microservices.
>
> This document only defines naming rules. Technical coding and architecture rules are documented in `BACKEND_CODING_GUIDELINES.md`.

## General C# naming

Follow standard .NET naming conventions and the repository `.editorconfig`.

| Element | Convention | Example |
|---|---|---|
| Namespace | PascalCase | `Dungeon.Application.Features.CreateDungeonRun` |
| Class | PascalCase | `DungeonRunRepository` |
| Record | PascalCase | `CreateDungeonRunCommand` |
| Interface | `I` + PascalCase | `IDungeonRunRepository` |
| Method | PascalCase | `GetByIdAsync` |
| Property | PascalCase | `DungeonRunId` |
| Private field | `_camelCase` | `_dungeonRunRepository` |
| Parameter | camelCase | `dungeonRunId` |
| Local variable | camelCase | `activeDungeonRun` |
| Constant | PascalCase | `MaximumPartySize` |

Treat abbreviations as normal words:

```text
ApiClient
HttpClient
JsonSerializer
UserId
```

Avoid:

```text
APIClient
HTTPClient
UserID
```

## Identifiers

Use:

```text
<Concept>Id
```

Examples:

```text
PlayerId
DungeonId
DungeonRunId
RewardId
ItemId
SessionId
MessageId
CorrelationId
CausationId
```

Do not use:

```text
IdPlayer
PlayerID
IDPlayer
```

Business and technical identifiers use `Guid` unless a documented exception exists.

## Dates and timestamps

Use `DateTimeOffset`.

Use consistent timestamp names:

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

Avoid synonyms for the same concept such as:

```text
CreationDate
DateCreated
CreatedDate
```

## Boolean properties

Boolean names should read naturally as questions.

Prefer:

```text
IsActive
IsCompleted
HasStarted
HasExpired
CanRetry
ShouldPublish
```

Avoid ambiguous names such as:

```text
Active
Completed
Retry
```

## Collections

Collection names must be plural.

```csharp
IReadOnlyCollection<Player> Players
IEnumerable<Item> Items
```

## Async methods

Asynchronous methods must end with `Async`.

```text
GetByIdAsync
GetAllAsync
CreateAsync
UpdateAsync
DeleteAsync
ExistsAsync
```

Use domain verbs when they add meaning:

```text
StartDungeonAsync
CompleteDungeonRunAsync
EquipItemAsync
GrantRewardAsync
```

Do not introduce synonyms such as `FetchById`, `RetrieveById`, or `LoadById` when the operation is simply `GetByIdAsync`.

## CQRS naming

Commands modify state:

```text
<CreateThing>Command
<CreateThing>CommandHandler
<CreateThing>Validator
```

Queries read state:

```text
<GetThing>Query
<GetThing>QueryHandler
```

Examples:

```text
CreatePlayerCommand
CreatePlayerCommandHandler
CreatePlayerValidator

GetPlayerByIdQuery
GetPlayerByIdQueryHandler
```

## API contract naming

Use explicit names.

Prefer:

```text
CreatePlayerRequest
PlayerDto
DungeonDto
DungeonRunDto
```

Avoid vague names such as:

```text
DataDto
ModelDto
ResponseDto
```

## Entity Framework naming

Entity configurations use:

```text
<Entity>Configuration
```

Examples:

```text
PlayerConfiguration
DungeonConfiguration
DungeonRunConfiguration
```

## Message naming

Events describe something that already happened and use past tense.

```text
PlayerCreated
DungeonStarted
DungeonRunCompleted
RewardGranted
ItemEquipped
ProgressionUpdated
```

Avoid vague names:

```text
PlayerEvent
DungeonMessage
UpdateEvent
```

Asynchronous commands, when needed, use imperative intent:

```text
GrantReward
RebuildLeaderboard
```

Logical message destinations use lowercase kebab-case:

```text
<domain>.<message>.v<version>
```

Examples:

```text
dungeon.dungeon-run-completed.v1
rewards.reward-granted.v1
dungeon.dungeon-started.v1
```

## Test naming

Tests use:

```text
Method_Scenario_ExpectedResult
```

Examples:

```text
Handle_ValidCommand_CreatesPlayer
Handle_PlayerDoesNotExist_ThrowsNotFoundException
GrantReward_DuplicateMessage_DoesNotGrantRewardTwice
```

## Final rule

Equivalent concepts must use the same vocabulary across all microservices.

When a name already exists for a concept, reuse it instead of creating a synonym.
