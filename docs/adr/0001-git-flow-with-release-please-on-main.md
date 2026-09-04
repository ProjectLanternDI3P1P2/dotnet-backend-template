# Git flow, with release-please targeting `main`

Work integrates on `dev` (the default branch) and is promoted to `main`, which
carries tags and `CHANGELOG.md`. release-please runs on `main` with an explicit
`target-branch: main`, because the action otherwise targets the repository's
default branch — which is `dev`.

## Considered Options

Running release-please on `dev` instead would have tagged versions before they
were promoted, putting the tag on a commit that had not yet reached the released
line. Keeping a single branch and dropping `dev` would have removed the
integration point the five services rely on for cross-cutting changes.

## Consequences

`main` and `dev` diverge at every release, which is what
[ADR-0003](./0003-automatic-back-merge-from-main-to-dev.md) exists to repair, and
the promotion merge is constrained by
[ADR-0002](./0002-merge-strategy-depends-on-the-target-branch.md).
