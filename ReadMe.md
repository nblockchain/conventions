This is a repository that contains several useful things that other `nblockchain` repos use, such as:

- [Config file](commitlint.config.ts) and [plugins](commitlint/plugins.ts) for commitlint.
- [A reference .editorconfig file](.editorconfig).
- [An F# style guide](FSharpStyleGuide.md).
- [Workflow guidelines](WorkflowGuidelines.md).
- Scripts that aid maintainability:
    * [Detection of .fsx scripts without shebang](scripts/shebangConvention.fsx).
    * [F# scripts compilation](scripts/compileFSharpScripts.fsx).
    * [EOF without EOL detection](scripts/eofConvention.fsx).
    * [Mixed line-endings detection](scripts/mixedLineEndings.fsx).
    * Use of unpinned versions:
        * [Use of `-latest` suffix in `runs-on:` GitHubCI tags](scripts/unpinnedGitHubActionsImageVersions.fsx).
        * [Use of asterisk (*) in `PackageReference` items of .NET projects](scripts/unpinnedDotnetPackageVersions.fsx).
        * [Missing the version number in `#r "nuget: ` refs of F# scripts](scripts/unpinnedNugetPackageReferenceVersions.fsx).

All in all, this is mainly documentation, and some tooling to detect bad practices.

More things to come:
- Detect .fsx files without +x attrib.
- Detect old versions of FSharpLint and fantomas/fantomless being used.
- Detect old versions of .editorconfig or Directory.Build.props being used.
- Detect unpinned versions, such as:
    * Missing the `--version` flag in `dotnet tool install foo` invocations.
