# Sonar treats `dev` as the reference branch

Sonar's main branch is set to `dev`, not `main`, so the quality gate on new code
is evaluated where code actually arrives: on pull requests targeting `dev`. With
`main` as the reference, the gate would only bite at promotion, after review and
merge, when it is too late to be acted on cheaply.

## Consequences

`dev` must be analysed on every push, not only on pull requests, or the reference
branch has no analysis to compare against. Because release tags live on `main`
(see [ADR-0001](./0001-git-flow-with-release-please-on-main.md)) and `dev` has
none, `dev` cannot use a "previous version" new-code definition — it uses a
rolling 30-day window, while `main` uses previous version.
