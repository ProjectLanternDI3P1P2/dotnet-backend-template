---
name: backend-persistence
description: Implement EF Core persistence, repositories, PostgreSQL access, dependency registration, and strongly typed application configuration.
---

# Backend Persistence

## EF Core ownership

Keep EF Core concerns in Infrastructure.

Do not add persistence-specific dependencies to Domain.

Prefer Fluent API configuration.

Use:

```text
<Entity>Configuration
```

Example:

```text
PlayerConfiguration
CombatConfiguration
```

Database-specific behavior must not leak into controllers or Domain logic.

## Repositories

Repository abstractions are placed with the application core according to the existing repository structure, currently under:

```text
Domain/Repositories/
```

Implement repositories in Infrastructure.

Reuse an existing repository abstraction when it already covers the concept.

Do not create a repository per handler without a clear domain need.

## Dependency injection

Use constructor injection.

Register technical dependencies close to Infrastructure.

Use the existing registration entry point:

```text
InfrastructureServiceRegistration.cs
```

Application registration remains in:

```text
ApplicationServiceRegistration.cs
```

Do not move all registrations into `Program.cs` when layer registration extensions already exist.

## Configuration

Access configuration through strongly typed `IOptions<T>`.

Prefer:

```csharp
IOptions<KeycloakOptions>
```

Do not scatter raw configuration lookups across the codebase.

Group options by technical concern.

Validate required configuration at startup when invalid values prevent correct service operation.

## Transactions and persistence behavior

Do not invent a project-wide transactional pattern that is not already defined.

Preserve existing persistence behavior unless the task explicitly changes it.
