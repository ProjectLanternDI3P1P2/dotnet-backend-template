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
rolling 30-day window.

`main` is not analysed at all. SonarQube Cloud's free plan covers exactly one
long-lived branch, and spending it on `main` would put the gate after the merge
instead of before it. In SonarQube Cloud the project's main branch is therefore
renamed to `dev`, and the Sonar workflow only triggers on `dev` and on pull
requests targeting it. Promotion pull requests into `main` carry no Sonar check —
by then the code has already passed the gate on its way into `dev`.

