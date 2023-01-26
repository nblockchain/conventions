This is a repository that contains several useful things that other `nblockchain` repos use, such as:

- [File with commitlint configuration and plugins](commitlint.config.ts).
- [A reference .editorconfig file](.editorconfig).
- [An F# style guide](FSharpStyleGuide.md).
- [Workflow guidelines](WorkflowGuidelines.md).

All in all, this is mainly documentation, and some tooling to detect bad practices.

More things to come:
- Detect .fsx files without shebang or +x attrib.
- Detect files with mixed line endings.
- Detect old versions of FSharpLint and fantomas/fantomless being used.
- Detect unpinned versions, such as:
    * Use of -latest in `runs-on:` GitHubCI tags.
    * Use of asterisk (*) in PackageReference items of .NET projects.
    * Missing the version number in `#r "nuget: ` refs of F# scripts.
    * Missing the `--version` flag in `dotnet tool install foo` invocations.
