# Merge strategy depends on the target branch

Pull requests into `dev` are squash-merged, so one feature becomes one
conventional commit. The promotion pull request from `dev` into `main` is merged
with a **merge commit**, never squashed: release-please reads the individual
commits to build the changelog and choose the version bump, and a squash would
collapse an entire release into a single entry.

## Consequences

GitHub configures allowed merge methods per repository, not per target branch, so
both methods stay enabled at repository level. The rule is enforced by branch
rulesets instead — see
[ADR-0007](./0007-merge-methods-enforced-by-rulesets.md). A
rebase merge is not an acceptable substitute: it rewrites commit SHAs, which
would permanently fork `dev` from `main` and break the back-merge in
[ADR-0003](./0003-automatic-back-merge-from-main-to-dev.md).
