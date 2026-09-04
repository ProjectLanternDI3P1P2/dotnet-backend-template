# Automatic back-merge from `main` to `dev`

release-please writes `CHANGELOG.md` and the version onto `main` only, so `dev`
immediately falls behind and the next promotion conflicts on exactly those files.
A workflow therefore opens and merges a `main` → `dev` pull request as soon as a
release lands.

## Consequences

This is automated rather than left to a checklist because the conflict is
guaranteed at every single release, and a manual step that must be performed
every time is a step that will eventually be forgotten. The back-merge needs
`main` to be mergeable into `dev` without a human, so branch protection on `dev`
must not require a linear history.
