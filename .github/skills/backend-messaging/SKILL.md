---
name: backend-messaging
description: Implement versioned asynchronous contracts, message handling, idempotency, logical destination naming, and safe remote-call resilience.
---

# Backend Messaging and Resilience

## Contracts

Serialize asynchronous messages as JSON.

Use common message metadata concepts:

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

Required concepts:

- `messageId`;
- `correlationId`;
- `causationId` when applicable;
- `messageType`;
- `version`;
- `occurredAt`;
- `producer`;
- `payload`.

Do not bind shared message contracts to broker-specific terminology unless required by the selected implementation.

## Event naming

Events use explicit past-tense business names.

Examples:

```text
PlayerCreated
DungeonStarted
CombatCompleted
RewardGranted
```

## Versioning

All message contracts are versioned.

Start at `v1`.

Keep backward-compatible additions in the current version.

Create a new version for breaking changes.

Do not treat an optional JSON field addition as breaking by default.

## Logical destination naming

Use lowercase kebab-case:

```text
<domain>.<message>.v<version>
```

## Idempotency

Assume distributed operations may be delivered or executed more than once.

Protect business invariants for duplicate operations.

Examples:

- do not grant the same unique reward twice;
- do not apply the same combat result twice;
- do not add the same progression event twice.

Use `MessageId` or a business idempotency key when appropriate.

## Remote calls

For synchronous remote calls, consider:

- cancellation;
- timeout;
- transient failures;
- retry safety;
- idempotency;
- cascading failures;
- degraded behavior.

Do not add retries blindly.

Only retry when the operation is safe to repeat or protected by idempotency.

Do not introduce a new project-wide resilience mechanism without an existing convention or explicit instruction.
