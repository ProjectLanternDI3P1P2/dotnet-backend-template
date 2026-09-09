# AI Backend Guidelines

> Entry point for AI coding agents working on backend repositories.
>
> Detailed rules are split into focused skills under `.github/skills/`.

Before modifying backend code:

1. inspect the existing repository structure and nearby implementations;
2. reuse existing vocabulary and conventions;
3. load the skills relevant to the requested change;
4. avoid unrelated refactors;
5. preserve existing public contracts unless a breaking change is explicitly requested.

## Skills

| Skill | Use when |
|---|---|
| `backend-naming` | Creating or renaming classes, methods, properties, DTOs, messages, tests, or identifiers |
| `backend-architecture` | Creating files, deciding layer ownership, changing project structure, or adding dependencies between layers |
| `backend-application` | Implementing CQRS commands, queries, handlers, MediatR flows, validators, or application orchestration |
| `backend-api` | Creating or changing controllers, routes, DTOs, API versions, validation responses, or HTTP error handling |
| `backend-persistence` | Changing EF Core models, configurations, repositories, PostgreSQL access, DI, or application configuration |
| `backend-messaging` | Creating messages, message handlers, versioned contracts, idempotent processing, or remote-call resilience |
| `backend-observability` | Adding or changing logs, tracing, correlation, OpenTelemetry, or cancellation propagation |
| `backend-testing` | Adding or modifying unit tests |
| `backend-code-quality` | Reviewing or modifying existing code, comments, formatting, dependencies, compatibility, or cross-cutting implementation quality |

## Required behavior

An agent must:

- follow `BACKEND_NAMING_CONVENTIONS.md`;
- follow `BACKEND_CODING_GUIDELINES.md`;
- follow the relevant skills;
- prefer existing repository conventions when compatible with these rules;
- preserve business invariants;
- preserve API and message compatibility unless explicitly instructed otherwise;
- add or update tests for changed behavior;
- report uncertainty instead of inventing a new project-wide convention.
