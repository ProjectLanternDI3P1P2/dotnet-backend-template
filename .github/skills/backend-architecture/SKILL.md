---
name: backend-architecture
description: Apply Clean Architecture boundaries and repository organization when creating files, projects, dependencies, or cross-layer code.
---

# Backend Architecture

Use this project structure:

```text
<Service>.Domain/
<Service>.Application/
<Service>.Infrastructure/
<Service>.Presentation/
<Service>.Test/
```

Dependencies must point inward:

```text
Presentation
 ├──> Application
 └──> Infrastructure

Infrastructure
 ├──> Application
 └──> Domain

Application
 └──> Domain

Domain
 └──> nothing
```

## Domain

Place business concepts and rules in Domain.

Typical folders:

```text
Entities/
Enums/
Exceptions/
Repositories/
Services/
```

Add `ValueObjects/` or `Events/` only when required.

Domain must not depend on:

- ASP.NET Core;
- Entity Framework Core;
- message brokers;
- HTTP clients;
- logging implementations;
- configuration providers.

Do not place business rules in controllers or Infrastructure.

## Application

Place use cases and orchestration in Application.

Organize code feature-first.

Do not create global `Commands/`, `Queries/`, `Handlers/`, and `Validators/` folders when the repository uses feature-first organization.

Application must not:

- depend on HTTP concepts;
- use `DbContext` directly;
- instantiate concrete HTTP or broker clients;
- read environment variables directly.

## Infrastructure

Place technical implementations in Infrastructure.

Infrastructure may contain:

- EF Core;
- PostgreSQL;
- repository implementations;
- HTTP integrations;
- message-broker integrations;
- authentication adapters.

Infrastructure must not contain business rules.

## Presentation

Place HTTP-specific code in Presentation.

Keep controllers thin.

Presentation may contain:

```text
Controllers/
DTO/
Extensions/
Middleware/
Program.cs
```

## Change discipline

Before creating a file:

1. inspect nearby features;
2. reuse the existing folder pattern;
3. reuse existing abstractions;
4. avoid introducing a new shared convention;
5. avoid unrelated refactors.

Do not introduce shared backend business or application libraries between microservices.
