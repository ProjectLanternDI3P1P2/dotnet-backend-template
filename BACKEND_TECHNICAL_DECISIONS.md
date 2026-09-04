# Backend Technical Decisions

> This document records backend technical decisions shared by all five microservices.
>
> It answers **"what do we use?"**, and carries the conventions that no tool can
> enforce — the ones about distributed correctness and shared contracts.
> Formatting and code style are not written down here: they are enforced by
> `.editorconfig` and `dotnet format` in CI and in the pre-commit hook.
>
> Decisions specific to *this* repository's shape — branching, releases, CI —
> live in [docs/adr](./docs/adr). Vocabulary lives in [CONTEXT.md](./CONTEXT.md).
>
> Decisions still under discussion MUST remain explicitly marked as open.

---

# Confirmed decisions

| Area | Decision |
|---|---|
| Language | C# |
| Runtime | .NET 10 |
| HTTP API | ASP.NET Core Controllers |
| HTTP API project suffix | `Presentation` |
| Architecture | Clean Architecture |
| Repository model | One repository per microservice |
| Shared backend library | None |
| Persistence | Entity Framework Core |
| Database | PostgreSQL |
| Application pattern | CQRS |
| Mediator | MediatR |
| Validation | FluentValidation |
| Error handling | Exceptions + global error middleware |
| Error response format | ProblemDetails |
| Validation error HTTP status | 422 |
| Identifier type | Guid |
| Date/time type | DateTimeOffset |
| Domain finite states | C# enums |
| API style | REST |
| API versioning | Version in URL |
| Initial API version | `v1` |
| API documentation | Scalar |
| Authentication | Keycloak |
| Logging | Serilog |
| Distributed observability | OpenTelemetry |
| Async message format | JSON |
| Async message metadata | Common envelope |
| Message contract versioning | Mandatory |
| Queue/topic logical naming | Common convention |
| Unit test framework | xUnit |
| Test runner | Microsoft.Testing.Platform |
| Mocking | Moq |
| Test data | Bogus |
| Assertions | FluentAssertions |
| Test naming | `Method_Scenario_ExpectedResult` |
| Configuration | `IOptions<T>` |
| Containers | Dockerfile per service |
| Health checks | `/health/live`, `/health/ready` |
| Code formatting | Standard .NET + repository `.editorconfig` |
| Static analysis | SonarQube Cloud |
| Code coverage | `dotnet-coverage` |
| Dependency updates | Dependabot |
| Branching model | git flow: `dev` integrates, `main` releases |
| Release automation | release-please, Conventional Commits |

---

# CQRS conventions

Commands modify state.

Queries read state.

Naming:

```text
<CreateThing>Command
<CreateThing>CommandHandler
<CreateThing>CommandValidator

<GetThing>Query
<GetThing>QueryHandler
```

Handlers MUST contain application orchestration, not HTTP-specific logic.

A command/query SHOULD represent one clear use case.

Controllers SHOULD communicate with Application through MediatR, and handlers
SHOULD receive dependencies through constructor injection. Avoid service locator
patterns, and avoid injecting `IServiceProvider` to resolve arbitrary
dependencies inside business or application code.

Validators follow the use case name (`CreateHeroCommandValidator`) and cover
input shape. True business invariants MUST remain protected by the Domain or
Application logic: validation MUST NOT be the only protection for a critical
business invariant.

---

# API contracts

API transport models SHOULD be separate from Domain entities.

Do not expose EF Core entities directly through controllers.

Request and response types SHOULD have explicit names when the API grows:

```text
CreateHeroRequest
HeroResponse
```

The `Dto` suffix MAY be used for simple transport models. Avoid vague names such
as `DataDto` or `ResponseDto` when a more precise name exists.

---

# API versioning policy

All HTTP APIs begin with:

```text
/api/v1/...
```

Backward-compatible changes remain in the current major version.

A breaking contract change requires a new major version.

Examples that normally remain in `v1`:

- adding a new endpoint;
- adding an optional request property;
- adding an optional response property.

Examples that may require `v2`:

- removing an existing required property;
- renaming a required property;
- changing the type or meaning of an existing property;
- changing an existing route incompatibly.

The technical ASP.NET Core package/configuration used to implement API version negotiation is not yet fixed.

---

# Messaging convention

The broker is not selected yet.

The message format is JSON.

Common metadata:

```json
{
  "messageId": "guid",
  "correlationId": "guid",
  "causationId": "guid",
  "messageType": "CombatCompleted",
  "version": 1,
  "occurredAt": "2026-09-02T12:00:00+00:00",
  "producer": "combat",
  "payload": {}
}
```

Logical message destinations use lowercase kebab-case following:

```text
<domain>.<message>.v<version>
```

Examples:

```text
combat.combat-completed.v1
rewards.reward-granted.v1
```

Avoid names tied to a specific developer, environment or implementation detail.

The actual exchange/topic/queue topology will be defined after the broker is selected.

## Message versioning

Message contracts MUST be versioned, starting at `v1`.

Backward-compatible additions SHOULD remain in the same version. A consumer MUST
NOT assume that adding a new optional JSON field is a breaking change.

Removing, renaming or changing the meaning or type of an existing required field
SHOULD be considered breaking, and requires a new version.

---

# Idempotency

Distributed operations MAY be retried or delivered more than once.

Handlers processing operations that can be duplicated MUST protect business
invariants:

- the same reward must not be granted twice;
- the same combat result must not be processed twice;
- the same progression event must not add XP twice.

Message handlers SHOULD use `messageId` or a business idempotency key where
appropriate.

Idempotency is a business correctness concern, not only a messaging concern.

---

# Health checks

Each service MUST expose health checks:

```text
/health/live
/health/ready
```

`live` indicates whether the process is alive. `ready` indicates whether the
service is ready to receive traffic.

Health checks MUST remain useful and MUST NOT perform unnecessarily expensive
operations.

---

# Tests

Test names MUST follow:

```text
Method_Scenario_ExpectedResult
```

Tests SHOULD focus on behaviour, and separate Arrange, Act and Assert. Avoid
tests that only mirror implementation details, and avoid mocking simple domain
objects.

---

# Open decisions

The following choices are intentionally not fixed yet.

## Inter-service communication

Open questions:

- HTTP REST?
- gRPC?
- another protocol?
- different communication styles depending on the use case?

No implementation should be standardised before this decision is made.

## Broker

The asynchronous broker is not yet selected.

The chosen broker must support the distributed-system requirements of the project and the shared messaging conventions.

## Resilience

No resilience library or standard policy is fixed yet.

Every synchronous remote integration must nevertheless evaluate:

- timeout;
- cancellation;
- transient failures;
- retry safety;
- idempotency;
- cascading failures;
- degraded behaviour.

Retries must never be enabled blindly. A retry is only safe when the operation
itself is safe to retry or protected by idempotency:

```text
Start combat
→ timeout
→ automatic retry
→ two combats created
```

Any resilience mechanism introduced MUST be documented and justified.

---

# Intentionally unspecified for now

The following topics are outside the current decision set:

- object mapping library;
- transactional outbox implementation;
- integration-test tooling;
- secrets management;
- database migration execution strategy;
- caching;
- feature flags;
- detailed broker topology.

These subjects should only be added once a real project need requires a shared decision.

---

# Decision rule

A technology choice affecting multiple repositories must be discussed as a project-wide decision before teams independently introduce incompatible solutions.

The absence of a shared library does not mean the absence of shared standards.

Any new confirmed decision should be added to this file. When the decision is
about how *this* repository builds, tests or releases itself, record it as an ADR
in [docs/adr](./docs/adr) instead.
