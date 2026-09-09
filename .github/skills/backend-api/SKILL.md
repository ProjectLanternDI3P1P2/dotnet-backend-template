---
name: backend-api
description: Implement or modify ASP.NET Core controllers, routes, DTOs, API versioning, validation responses, and global HTTP error handling.
---

# Backend API

## Controllers

Controllers must remain thin.

A controller should:

1. receive the HTTP request;
2. map it to a command or query;
3. send it through MediatR;
4. return the HTTP response.

Controllers must not contain:

- business rules;
- EF Core queries;
- persistence logic;
- external-service implementation logic;
- repeated `try/catch` blocks for standard application errors.

## Routes

Use REST-oriented resource routes.

Prefer:

```text
GET    /api/v1/players/{playerId}
POST   /api/v1/players
PUT    /api/v1/players/{playerId}
DELETE /api/v1/players/{playerId}
```

Avoid:

```text
POST /api/v1/createPlayer
GET  /api/v1/getPlayer
```

Rules:

- start routes with `/api/v1/`;
- use lowercase plural resource names;
- use explicit route parameter names when useful;
- use nouns rather than action names when possible.

## Versioning

Backward-compatible changes remain in the current major version.

Examples:

- add a new endpoint;
- add an optional request property;
- add an optional response property.

Breaking changes require a new major version.

Examples:

- remove or rename a required property;
- change a property's type or meaning;
- change an existing route incompatibly.

## DTOs

Keep HTTP transport models in `Presentation/DTO`.

Do not expose Domain or EF Core entities directly.

Use explicit DTO or request names.

## Error handling

Use centralized error handling middleware.

Use `ProblemDetails`.

Default mapping:

| Situation | HTTP |
|---|---:|
| Authentication failure | 401 |
| Resource not found | 404 |
| Conflict / invalid state | 409 |
| Validation failure | 422 |
| Unexpected technical error | 500 |

Do not expose internal exception details or stack traces in production.

Log unexpected exceptions.

## Cancellation

Controllers must pass `CancellationToken` to MediatR.
