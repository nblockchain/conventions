This is a repository that contains several useful things that other `nblockchain` repos use, such as:

- [Config file](commitlint.config.ts) and [plugins](commitlint/plugins.ts) for commitlint.
- [A reference .editorconfig file](.editorconfig).
- [An F# style guide](FSharpStyleGuide.md).
- [Workflow guidelines](WorkflowGuidelines.md).
- Useful scripts to detect bad practices, such as:
    * [EOF without EOL](scripts/eofConvention.fsx).
    * [Mixed line-endings](scripts/mixedLineEndings.fsx).
    * [.fsx scripts without shebang](scripts/shebangConvention.fsx).
    * Use of unpinned versions:
        * [Use of `-latest` suffix in `runs-on:` GitHubCI tags](scripts/unpinnedGitHubActionsImageVersions.fsx).
        * [Use of asterisk (*) in `PackageReference` items of .NET projects](scripts/unpinnedDotnetPackageVersions.fsx).
        * [Missing the version number in `#r "nuget: ` refs of F# scripts](scripts/unpinnedNugetPackageReferenceVersions.fsx).
    * [Compile F# scripts](scripts/compileFsharpScripts.fsx).

All in all, this is mainly documentation, and some tooling to detect bad practices.

More things to come:
- Detect .fsx files without +x attrib.
- Detect old versions of FSharpLint and fantomas/fantomless being used.
- Detect unpinned versions, such as:
    * Missing the `--version` flag in `dotnet tool install foo` invocations.
