# Merge methods are enforced per branch by rulesets

The rule in [ADR-0002](./0002-merge-strategy-depends-on-the-target-branch.md) could
only ever be a convention: GitHub configures allowed merge methods per repository,
not per branch. Two rulesets now enforce it — `dev` accepts squash only, `main`
accepts merge commits only — so a promotion can no longer be squashed by accident.

## Consequences

The automatic back-merge ([ADR-0003](./0003-automatic-back-merge-from-main-to-dev.md))
merges `main` into `dev` with a merge commit, which the `dev` ruleset would refuse.
Organisation admins therefore bypass the `dev` ruleset, since that workflow runs with
an admin's token.

Two costs, accepted knowingly. An admin still sees the merge-commit button on `dev`,
so the rule protects everyone except the people most likely to be promoting releases.
And the bypass hangs on a *role*, not on the workflow: whoever is granted admin later
inherits it without anyone deciding so.

The alternative — squashing the back-merge too — would have removed the exception
entirely, at the price of `dev` and `main` no longer sharing commit history. Keeping
the shared history was judged worth one bypass.
