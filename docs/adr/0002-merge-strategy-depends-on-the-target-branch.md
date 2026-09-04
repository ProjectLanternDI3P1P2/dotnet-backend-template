# Merge strategy depends on the target branch

Pull requests into `dev` are squash-merged, so one feature becomes one
conventional commit. The promotion pull request from `dev` into `main` is merged
with a **merge commit**, never squashed: release-please reads the individual
commits to build the changelog and choose the version bump, and a squash would
collapse an entire release into a single entry.

## Consequences

GitHub configures allowed merge methods per repository, not per target branch, so
both methods stay enabled and nothing mechanically prevents a squashed promotion.
The rule is held by convention and by the promotion checklist in the README. A
rebase merge is not an acceptable substitute: it rewrites commit SHAs, which
would permanently fork `dev` from `main` and break the back-merge in
[ADR-0003](./0003-automatic-back-merge-from-main-to-dev.md).
