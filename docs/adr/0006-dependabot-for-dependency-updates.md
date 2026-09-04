# Dependabot for dependency updates

Package-management automation was previously listed as intentionally unspecified.
It is now decided: Dependabot keeps NuGet packages and GitHub Actions up to date
in every service repository.

## Consequences

Minor and patch updates are grouped into a single pull request to keep review
cost low; major updates keep their own pull request, because they are the ones
worth reading a changelog for. GitHub Actions are updated monthly rather than
weekly, since they are pinned by commit SHA and move slowly.
