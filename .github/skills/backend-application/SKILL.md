---
name: backend-application
description: Implement application use cases with CQRS, MediatR, FluentValidation, dependency injection, and application orchestration.
---

# Backend Application

## Feature organization

Use feature-first organization.

Example:

```text
Features/
└── PlayerUseCase/
    ├── CreatePlayer/
    │   ├── CreatePlayerCommand.cs
    │   ├── CreatePlayerCommandHandler.cs
    │   └── CreatePlayerValidator.cs
    └── GetPlayerById/
        ├── GetPlayerByIdQuery.cs
        └── GetPlayerByIdQueryHandler.cs
```

Follow the exact nearby repository pattern when it already exists.

## CQRS

Commands modify state.

Queries read state.

A command or query should represent one clear use case.

Do not create generic operations such as:

```text
ManagePlayerCommand
PlayerOperationCommand
```

when a specific business intent exists.

## Handlers

Handlers may:

- load required data through abstractions;
- coordinate Domain behavior;
- verify application-level conditions;
- persist through abstractions;
- return application results.

Handlers must not:

- use `HttpContext`;
- return `IActionResult`;
- select HTTP status codes;
- access `DbContext` directly;
- instantiate concrete clients;
- contain broker-specific code;
- read raw environment variables.

## MediatR

Controllers should dispatch commands and queries through MediatR.

```csharp
var result = await sender.Send(request, cancellationToken);
```

Use constructor injection.

Do not use `IServiceProvider` as a service locator.

## Validation

Use FluentValidation for request and input constraints.

Name validators after the use case:

```text
CreatePlayerValidator
StartDungeonValidator
```

Keep critical business invariants enforced in Domain or Application behavior.

Validation must not be the only protection for critical business rules.

## Cancellation

Accept and propagate `CancellationToken` when cancellation is meaningful.

Do not silently drop a token in a call chain that supports cancellation.

## Dependencies

Reuse existing abstractions.

Do not add a new dependency unless it is necessary for the requested behavior.
