This is a repository that contains several useful things that other `nblockchain` repos use, such as:

- [Config file](commitlint.config.ts) and [plugins](commitlint/plugins.ts) for commitlint.
- [A reference .editorconfig file](.editorconfig).
- [An F# style guide](docs/FSharpStyleGuide.md).
- [Workflow guidelines](docs/WorkflowGuidelines.md).
- Scripts that aid maintainability:
    * [Detection of .fsx scripts without shebang](scripts/shebangConvention.fsx).
    * [F# scripts compilation](scripts/compileFSharpScripts.fsx).
    * [EOF without EOL detection](scripts/eofConvention.fsx).
    * [Mixed line-endings detection](scripts/mixedLineEndings.fsx).
    * [Auto-wrap the latest commit message](scripts/wrapLatestCommitMsg.fsx).
    * Use of unpinned versions:
        * [Use of `-latest` suffix in `runs-on:` GitHubCI tags](scripts/unpinnedGitHubActionsImageVersions.fsx).
        * [Use of asterisk (*) in `PackageReference` items of .NET projects](scripts/unpinnedDotnetPackageVersions.fsx).
        * [Missing the version number in `#r "nuget: ` refs of F# scripts](scripts/unpinnedNugetPackageReferenceVersions.fsx).
        * [Missing the `--version` flag in `dotnet tool install foo` invocations](scripts/unpinnedDotnetToolInstallVersions.fsx).

All in all, this is mainly documentation, and some tooling to detect bad practices.

More things to come:
- Detect .fsx files without +x attrib.
- Detect old versions of FSharpLint and fantomas/fantomless being used.
- Detect old versions of .editorconfig or Directory.Build.props being used.
- Detect non-verbose flags (e.g. `dotnet build -c Debug` instead of `dotnet build --configuration Debug`) being used in scripts or YML CI files (there are exceptions, e.g. `env -S`).
