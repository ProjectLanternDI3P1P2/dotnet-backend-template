# .NET Backend Service Template

Starting point for a backend microservice: a .NET 10 Clean Architecture solution,
the CI pipeline that guards it, and the branching flow that releases it.

Vocabulary is defined in [CONTEXT.md](./CONTEXT.md). Shared technical choices are
recorded in [BACKEND_TECHNICAL_DECISIONS.md](./BACKEND_TECHNICAL_DECISIONS.md),
and the decisions behind this repository's own shape in [docs/adr](./docs/adr).

## Structure

```text
Combat.Domain/          entities, enums, domain services, repository interfaces
Combat.Application/     commands, queries, handlers, validators, pipeline behaviours
Combat.Infrastructure/  EF Core, repository implementations, external services
Combat.Presentation/    HTTP API: controllers, DTOs, middleware
Combat.Test/            xUnit tests for all of the above
```

`Presentation` is the Clean Architecture layer name for the HTTP API. There is no
user interface.

## Commands

```powershell
dotnet tool restore
dotnet restore Combat.Presentation.slnx
dotnet build Combat.Presentation.slnx
dotnet test --solution Combat.Presentation.slnx
dotnet run --project Combat.Presentation/Combat.Presentation.csproj
```

The SDK version is pinned in `global.json`; `dotnet tool restore` installs the
coverage collector and the git-hook runner declared in `dotnet-tools.json`.
Run `dotnet husky install` once per clone to enable the pre-commit hook — git
hook paths are local configuration and cannot be committed.

## Configuration

PostgreSQL is configured through the `ConnectionStrings` section.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=combat;Username=combat",
    "PasswordFile": "/run/secrets/postgres_password"
  }
}
```

`PasswordFile` is optional. It injects the password from a Docker or Kubernetes
secret instead of storing it in the configuration file.

## Branching flow

```text
feature/xxx --squash--> dev --merge commit--> main --> tag + CHANGELOG
                         ^                      |
                         +----- back-merge -----+
```

- `dev` is the default branch. Open every feature pull request against it, and
  **squash** on merge: one feature becomes one conventional commit.
- Promote by opening a pull request from `dev` to `main` and merging it with a
  **merge commit**. Never squash this one — release-please reads the individual
  commits ([ADR-0002](./docs/adr/0002-merge-strategy-depends-on-the-target-branch.md)).
- release-please then maintains a release pull request on `main`. Merging it
  writes the changelog, bumps the version and tags.
- A back-merge from `main` to `dev` follows automatically
  ([ADR-0003](./docs/adr/0003-automatic-back-merge-from-main-to-dev.md)).

Commit messages follow [Conventional Commits](https://www.conventionalcommits.org):
`feat:` and `fix:` appear in the changelog and move the version, everything else
(`chore:`, `ci:`, `refactor:`, `test:`, `docs:`, `build:`, `style:`) is hidden and
moves nothing.

## Continuous integration

| Workflow | Runs on | Does |
| --- | --- | --- |
| `ci.yaml` | PR and push to `dev` / `main` | Calls the reusable lint, test and build workflows |
| `sonar.yaml` | PR and push to `dev`, except Dependabot | Builds and tests under the SonarScanner for .NET, uploads coverage |
| `security.yml` | PR and push to `dev` / `main` | Trivy filesystem scan, zizmor workflow audit |
| `release-please.yaml` | push to `main` | Maintains the release pull request |
| `back-merge.yaml` | after a release | Opens and merges `main` → `dev` |

Formatting is enforced by `dotnet format --verify-no-changes --severity warn`,
which reads `.editorconfig`. The same command runs locally as a pre-commit hook
through Husky.Net, installed by `dotnet tool restore`.

## Adding integration tests

There are none yet, and `Combat.Test` holds unit tests only —
`PlayerRepositoryTests` uses the EF Core in-memory provider, which is not a real
database. Real integration tests would need a `WebApplicationFactory` for the
HTTP surface and a containerised PostgreSQL for persistence.

Both are cross-cutting choices affecting all five services, so pick them as a
shared decision and record an ADR before adding them here. Once they exist, give
them their own job in `ci.yaml` so a slow suite does not gate the fast feedback
from lint and unit tests.

## Setting up a new repository from this template

1. Create the repository **public** (SonarQube Cloud's free tier requires it).
2. Import the organisation into SonarQube Cloud and create the project, then:
   - **Branches**: delete the `dev` entry SonarQube Cloud created on its own,
     then rename the main branch from `main` to `dev` — the rename is refused
     while a branch of that name already exists. The free plan covers one
     long-lived branch, and `dev` is the one that matters (ADR-0005).
   - **Administration → Analysis method**: switch **Automatic Analysis off**.
     Left on, it competes with the scanner and every CI analysis fails.
   - **Administration → New code**: number of days, 30.
3. Add `SONAR_TOKEN` and `BOT_TOKEN` as secrets. `BOT_TOKEN`, not the default
   `GITHUB_TOKEN`: a pull request opened by the latter triggers no workflow, so
   the release pull request would never get a CI run.
4. Set `dev` as the default branch and protect both `dev` and `main`. Required
   checks: `Lint / dotnet format`, `Test / dotnet test`, `Build / dotnet build`,
   `Trivy Security Scan`, `GitHub Actions audit`. **Not** `SonarQube Cloud scan`:
   it is skipped on Dependabot pull requests, and a required check that never
   runs blocks them forever. Keep "require linear history" **off**, or the merge
   commits this flow depends on become impossible.
5. Enable auto-merge on the repository; the back-merge workflow uses it.
6. Rename the `Combat.*` projects to your service name, and update `/k:` and
   `/o:` in `.github/workflows/sonar.yaml`.
