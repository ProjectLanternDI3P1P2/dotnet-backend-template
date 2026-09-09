---
name: backend-code-quality
description: Apply cross-cutting implementation rules for safe modifications, formatting, comments, compatibility, dependencies, and final review.
---

# Backend Code Quality

## Before editing

Inspect:

- the existing project structure;
- neighboring features;
- existing abstractions;
- vocabulary already used;
- tests for the affected behavior.

Reuse existing patterns when compatible with project rules.

## Change scope

Preserve working behavior outside the requested change.

Do not perform unrelated refactors.

Do not reformat unrelated files.

Do not create new abstractions without a clear need.

## Dependencies

Reuse existing dependencies when they meet the need.

Do not add a package only to reduce a small amount of code.

Do not introduce a cross-project dependency or convention silently.

## Comments

Comments should explain why.

Do not add comments that repeat what the code says.

## Public compatibility

Check whether a change affects:

- HTTP routes;
- DTOs;
- public request or response contracts;
- distributed messages;
- message versions;
- persisted behavior.

Preserve compatibility unless a breaking change is explicitly requested.

## Correctness checks

Do not assume a change is correct only because:

- it compiles;
- existing tests pass;
- EF Core accepts the model;
- an HTTP endpoint responds;
- a message serializes.

Check:

- business invariants;
- architecture boundaries;
- compatibility;
- idempotency when relevant;
- cancellation;
- error behavior;
- structured logging;
- tests.

## Uncertainty

Do not invent a new project-wide convention when no rule exists.

Use the smallest local solution that does not create a global standard.

Surface uncertainty when a shared decision is required.

## Final checklist

Before finishing, verify:

- code is in the correct layer;
- Clean Architecture dependencies are respected;
- naming conventions are followed;
- existing vocabulary is reused;
- controllers remain thin;
- handlers contain no HTTP logic;
- Infrastructure contains no business rules;
- critical invariants are not protected only by validators;
- async methods use `Async`;
- cancellation is propagated when relevant;
- logs are structured;
- secrets are not logged;
- public contracts remain compatible unless explicitly changed;
- replayable operations were evaluated for idempotency;
- tests cover changed behavior;
- unrelated code was not refactored.
