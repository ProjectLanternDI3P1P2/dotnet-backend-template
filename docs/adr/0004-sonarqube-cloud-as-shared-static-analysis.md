# SonarQube Cloud as the shared static-analysis tool

`BACKEND_TECHNICAL_DECISIONS.md` previously listed static-analysis tooling as
intentionally unspecified. It is now decided: every service repository runs
SonarQube Cloud, in the free tier, which requires the repositories to stay
public.

## Considered Options

Leaving it open would have let each of the five services adopt a different
analyser, producing quality reports that cannot be compared — the exact outcome
the project's decision rule exists to prevent.

## Consequences

C# is analysed by the SonarScanner for .NET, which wraps the MSBuild build
(`begin` → `build` → `test` → `end`) and needs a JDK on the runner. It ignores
`sonar-project.properties` entirely: settings are passed as `/d:` arguments in
the workflow. Coverage is collected with `dotnet-coverage` and imported through
`sonar.cs.vscoveragexml.reportsPaths`, because Sonar does not read Cobertura for
C#.
