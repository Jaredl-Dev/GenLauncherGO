# Contributing to GenLauncherGO

Contributions are welcome. Fork the repository, create a branch, make and test your changes, then open a pull request
with a clear description.

## Development setup

The repository selects the .NET 10 SDK through `global.json`, starting at version `10.0.300` and allowing later
feature bands.

Run the Avalonia project from the repository root:

```powershell
dotnet run --project ./GenLauncherGO.UI/GenLauncherGO.UI.csproj
```

For a full launcher UI session, run the executable outside all supported game installations. Startup validation
blocks the launcher when it is placed inside one.

## Required quality gates

Run the standard repository checks before submitting a change:

```powershell
dotnet build GenLauncherGO.sln
dotnet format GenLauncherGO.sln --verify-no-changes
dotnet test GenLauncherGO.sln
```

The Windows CI workflow builds Release, verifies formatting, runs the complete test suite with coverage thresholds,
checks each mutation-test area, and verifies the supported single-file publish profile. A separate weekly workflow
audits vulnerable and deprecated dependencies.

## Symbolic-link safety tests

Symbolic-link tests are required in CI and fail the workflow if the runner cannot execute them. Local accounts that
cannot create symbolic links report those tests as explicit skips. To enforce the same fail-closed behavior locally,
enable Windows Developer Mode or use an elevated terminal, then run:

```powershell
$env:GENLAUNCHERGO_REQUIRE_SYMBOLIC_LINK_TESTS = "true"
dotnet test GenLauncherGO.sln
Remove-Item Env:GENLAUNCHERGO_REQUIRE_SYMBOLIC_LINK_TESTS
```

## Coverage and mutation testing

Generate a local coverage report:

```powershell
dotnet msbuild ./eng/coverage.proj -target:Coverage
```

The HTML report is written to `artifacts/coverage/index.html`. Coverage thresholds catch missing execution; they do
not measure assertion quality.

Mutation-test the domain and infrastructure projects:

```powershell
dotnet msbuild ./eng/mutation.proj -target:Mutation
```

Run one area instead of all seven:

```powershell
dotnet msbuild ./eng/mutation.proj -target:Mutation -property:MutationConfiguration=infrastructure-launching
```

Mutation testing is the gate for whether behavior is meaningfully asserted. It covers every behavior-bearing file in
`GenLauncherGO.Core` and `GenLauncherGO.Infrastructure`, split across `core`, `infrastructure-common`,
`infrastructure-integrity`, `infrastructure-launching`, `infrastructure-mods`, `infrastructure-platform`, and
`infrastructure-updating`. Each area has its own break threshold, so a strongly covered area cannot hide a weaker one.
CI runs these areas as a matrix.

`GenLauncherGO.UI` is not mutation-tested because Avalonia emits `InitializeComponent` and the `x:Name` backing
fields through a Roslyn source generator. Stryker rebuilds from parsed syntax trees without running generators, so
the generated members are unavailable and the run aborts before testing anything. UI quality is enforced through the
coverage backstop and behavioral tests instead.

Some mutants survive intentionally. Exception-message text, `ConfigureAwait`, durability flags such as `Flush(true)`,
buffer sizes, and argument guards are either unobservable through behavior tests or excluded by the test guidance.
The Stryker configuration filters what it can, and the thresholds account for the remainder.

## Publishing

Publish the supported self-contained, single-file Windows x64 executable:

```powershell
dotnet publish ./GenLauncherGO.UI/GenLauncherGO.UI.csproj -p:PublishProfile=WinX64SelfContained -o ./publish
```

Ordinary Debug and Release builds are framework-dependent and do not select a runtime. The explicit
`WinX64SelfContained` profile produces the supported distributable, with `GenLauncherGO.exe` as the launcher
executable.

Create the release ZIP from that supported publish:

```powershell
./eng/package-release.ps1
```

The script writes `artifacts/GenLauncherGO.zip`, containing `GenLauncherGO/GenLauncherGO.exe`, and leaves the
matching package folder at `artifacts/GenLauncherGO`.

## Architecture

The solution deliberately uses three production projects, one test project, and one test-only analyzer project, with
no `src` folder:

| Project | Responsibility |
| --- | --- |
| `GenLauncherGO.Core` | Dependency-light contracts, launcher rules, models, and path identities |
| `GenLauncherGO.Infrastructure` | Disk, network, archive, process, persistence, integrity, and package-provider implementations |
| `GenLauncherGO.UI` | Native Avalonia presentation, user workflows, localization, and the dependency-injection composition root |
| `GenLauncherGO.Tests` | Observable behavior, compatibility, recovery, and file-system safety tests |
| `GenLauncherGO.TestAnalyzers` | Test-only analyzer enforcing the repository's test-method naming convention |

Dependencies point inward: UI can reference Core and Infrastructure, Infrastructure can reference Core, and Core does
not reference Avalonia or implementation packages. Interfaces represent intentional project or side-effect
boundaries; feature-internal code normally uses sealed concrete types.

Mutable paths carry their owning root so file operations can reject traversal and reparse-point escapes.

## Backend compatibility

GenLauncherGO consumes an external backend tied to [p0ls3r](https://github.com/p0ls3r) and the original GenLauncher
project. This repository does not control that backend, so its legacy remote YAML names and structure are preserved
exactly at the Infrastructure boundary and mapped into the application's internal models. Do not rename or reshape
that manifest contract without a deliberate compatibility plan coordinated with the backend maintainers.

## Submitting changes

Keep changes focused, preserve existing behavior unless the change deliberately updates it, and include tests for
observable behavior or safety invariants. In the pull request, explain what changed and list the verification you ran.
