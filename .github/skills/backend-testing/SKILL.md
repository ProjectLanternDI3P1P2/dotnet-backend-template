---
name: backend-testing
description: Create and maintain backend unit tests using xUnit, Moq, Bogus, FluentAssertions, and shared test conventions.
---

# Backend Testing

Use:

- xUnit;
- Moq;
- Bogus;
- FluentAssertions.

## Naming

Use:

```text
Method_Scenario_ExpectedResult
```

Examples:

```text
Handle_ValidCommand_CreatesPlayer
Handle_PlayerDoesNotExist_ThrowsNotFoundException
GrantReward_DuplicateMessage_DoesNotGrantRewardTwice
```

## Structure

Separate:

```text
Arrange
Act
Assert
```

## Scope

Test observable behavior.

Do not create tests that only mirror implementation details.

Add or update tests when behavior changes.

## Moq

Use Moq for dependency behavior.

Do not mock simple Domain objects unnecessarily.

## Bogus

Use Bogus when realistic test data improves readability.

## Assertions

Prefer FluentAssertions for expressive assertions.

## Business behavior

Include tests for business invariants relevant to the change.

For replayable or distributed behavior, include duplicate-processing cases when applicable.
