# Workflow guidelines

* When contributing a PullRequest, separate your commits in units of work (don't mix changes that have different concerns in the same commit). Don't forget to include all explanations and reasonings in the commit messages, instead of just leaving them as part of the PullRequest description.

* The most common mistake when mixing multiple concerns in the same commit is to join both refactorings and change of behaviour in the same change. As a convention, we will consider the word "refactoring" to refer to changes in the code that should not have any impact on the general behaviour of the program (especially talking about behaviour noticed by the end-user).

* Push each commit separately (instead of sending more than 1 commit in a single push), so that we can have a CI status for each commit in the PullRequest. This is a best practice because it will make sure that the build is not broken in between commits (otherwise, future developers may have a hard time when trying to bisect bugs). If you have already pushed your commits to the remote in one push, this can be re-done by using our [gitPush1by1.fsx](https://github.com/nblockchain/conventions/blob/master/scripts/gitPush1by1.fsx) script, or this technique manually: https://stackoverflow.com/a/3230241/544947

* In general, we prefer verbose code (even if it's longer) than short & clever code. This means:
    * We dislike short variable names (if there's some ambiguity on what your variable represents, then choose a longer and more descriptive name).
    * In languages that have optional braces for `if/else` statements (e.g. C#, TypeScript), we prefer to add them even if the code block will only contain one line. This way, when the next developer adds more lines to it later it's less work for him and doesn't cause unnecessary git-blame noise.

* Group import declarations (e.g. `open` in F# and `using` in C#) in three buckets:
    * The first group for the namespaces that come from the base class libraries.
    * Second group for external libraries (e.g. nuget packages).
    * Last for the namespaces that belong to the source code that lives in same repository.

* Avoid typical bad practices like:

    * Magic numbers:

      Avoid using unnamed numerical constants in software code, this practice makes code hard to understand and maintain.

      Example (with bad practice):
      ```csharp
      var distance = GpsUtil.GetDistance();
      if (distance < 100)
         throw new NotImplementedException();
      ```

      Improved code:
      ```csharp
      private const int MinimumSupportedDistanceToNotifyKillerDrones = 100;

      ...

      var distance = GpsUtil.GetDistance()
      if (distance < MinimumSupportedDistanceToNotifyKillerDrones)
         throw new NotImplementedException();
      ```

    * DRY (Don't Repeat Yourself):

      The DRY principle suggests that a piece of information should only be stored once in a project and should be referenced as needed, rather than being copied and pasted multiple times throughout the codebase.

      It has several benefits, including reducing the amount of code that needs to be written and maintained, improving the consistency and quality of the code, and reducing the risk of introducing errors and bugs when the information changes.

      Example (with bad practice):
      ```fsharp
      let preAuthInputMac =
         CalculateMacWithSHA3256
             preAuthInput
             ":hs_mac"

      ...

      let authInputMac =
         CalculateMacWithSHA3256
             authInput
             ":hs_mac"
      ```

      Improved code:
      ```fsharp
      let AuthenticationDigestCalculationKey = ":hs_mac"

      ...

      let preAuthInputMac =
         CalculateMacWithSHA3256
             preAuthInput
             AuthenticationDigestCalculationKey

      ...

      let authInputMac =
         CalculateMacWithSHA3256
             authInput
             AuthenticationDigestCalculationKey
      ```

    * Primitive Obsession:

      Primitive Obsession is a situation where simple data types such as strings, integers, or arrays are overused in place of more appropriate objects.
      
      Example (with bad practice):
      ```
      let saveFilePath = System.Console.ReadLine()
      
      let savedData = System.IO.File.ReadAllText saveFilePath
      ```

      Improved code:
      ```fsharp
      let saveFilePath =
         let saveFilePathInString =
             System.Console.ReadLine()
         System.IO.FileInfo saveFilePathInString

      let savedData = System.IO.File.ReadAllText saveFilePath.FullName
      ```

    * Discarding generic exceptions:

        Discarding generic exceptions is a bad practice because it can lead to unexpected behavior and bugs. It's better to handle the exception using a non-generic catch, or at least log them to help with debugging if something unexpected happens.
    
        Example (with bad practice):
        ```fsharp
        let Func (zipFile: FileInfo) =
            try
                ZipFile.Open(zipFile.FullName, ZipArchiveMode.Read) |> ignore<ZipArchive>
            with
            | _ -> 
                ()
        ```

        Improved code:
        ```fsharp
        let Func (zipFile: FileInfo) =
            try
                ZipFile.Open(zipFile.FullName, ZipArchiveMode.Read) |> ignore<ZipArchive>
            with
            | :? InvalidDataException -> 
                Console.Error.WriteLine $"Not a zip file %s{zipFile.FullName}"
            | :? FileNotFoundException -> 
                Console.Error.WriteLine $"File not found %s{zipFile.FullName}"
            | _ -> 
                reraise()
        ```

    * Catching preventable exceptions:

        There are exceptions that cannot really be predicted from the caller of the function and to handle them there is no other way than using `catch` (C#) or `with` (F#) blocks. However, there are exceptions that are completely preventable and should never be caught:

        Example (with bad practice):
        ```fsharp
            try
                name.ComposeFullName(separator)
            with
            | :? ArgumentNullException ->
                Console.Error.WriteLine "There was a problem retreiving the name"
            | :? NullReferenceException ->
                Console.Error.WriteLine "There was a problem in the naming system, please report this bug"
        ```

        Improved code:
        ```fsharp
            if isNull name then
                Console.Error.WriteLine "There was a problem in the naming system, please report this bug"
            elif isNull separator then
                Console.Error.WriteLine "There was a problem retreiving the name"
            else
                name.ComposeFullName separator
        ```

        This way, the code is more performant (even though this is the least important aspect), readable (as we can immediately see what is the potentially problematic variable) and maintainable (as new unexpected but unrelated exceptions can be generated by future versions of the library that contains the function called).


    * Not benefiting from your type system:

        We use statically-typed languages (such as TypeScript and C#, as opposed to JavaScript and Python) so that our code can be
        better protected by the compiler (instead of having to to cover every possible scenario with tests). Therefore, please
        make use of the type system whenever you can, for example:

        * Do not use edge-values to denote absence of value. Example: use null (`Nullable<DateTime>`) instead of `DateTime.MinValue`.
        * Do not use `undefined` which is a pitfall from JavaScript (the fact that it has two kinds of null values is a defect in
        its design). As we're using TypeScript we should be able to avoid the ugliness of JavaScript.

        Example (with bad practice):
        ```typescript
        if (foo === undefined || foo === null)
        {
            return 0;
        }
        return 1;
        ```

        Improved code:
        ```typescript
        if (TypeHelpers.IsNullOrUndefined(foo))
        {
            return 0;
        }
        return 1;
        ```

        * Use Option types instead of Nullable ones if your language provides it (e.g. if you're using F# instead of C#).

        Example (with bad practice):
        ```typescript
        if (TypeHelpers.IsNullOrUndefined(foo))
        {
            return 0;
        }
        else
        {
            return 1;
        }
        ```

        Improved code:
        ```typescript
        let bar = OptionStatic.OfObj(option);
        if (bar instanceof None) {
            return 0;
        }
        return 1;
        ```
     
    * Abusing obscure operators or the excessive multi-facetedness of basic ones:
 
        * Do not use the `!` operator for non-boolean types. For example, C language (and others inspired by it, such as TypeScript) allows to use ! for null-checks; however, writing the word `null` is much more readable than not writing it (there's a reason why C# didn't bring this feature from C).

        Example (with bad practice):
        ```typescript
        void Func(zipFile: SomeFileType)
        {
            if (!zipFile)
                return;
            ...
        ```

        Improved code:
        ```typescript
        void Func(zipFile: SomeFileType)
        {
            if (zipFile === null)
                return;
            ...
        ```

        * Do not use the new operators introduced in the latest versions of C#, which are completely unreadable and always require checking the documentation: `??`, `??=`, etc.
     
        Example (with bad practice):
        ```csharp
        someField = someValue ?? throw new ArgumentNullException("someValue cannot be null");

        someVar ??= expression;
        ```

        Improved code:
        ```csharp
        if (someValue is null) {
            throw new ArgumentNullException("someValue cannot be null");
        }
        someField = someValue;

        if (someVar is null) {
            someVar = expression;
        }
        ```
        
        * Avoid using spread operator (`...`) in JavaScript/TypeScript, because it's not present in other languages and can be confusing. Prefer using `Array.from` methods for creating arrays from other sequences.
        
        Example (with bad practice):
        ```typescript
        const inputs = [...tableCell.children];
        ```

        Improved code:
        ```typescript
        const inputs = Array.from(tableCell.children);
        ```

* If you want to contribute a script, do not use PowerShell or Bash, but
an F# script. The reason to not use PowerShell is a personal preference
from the maintainer of this project (and his suspicion that it might not
be 100% guaranteed to be crossplatform); and the reason not to use Bash
is because it's only Unix compatible (we cannot assume WSL is installed
in Windows), and in general because it's too undeterministic and old, more
info here: https://news.ycombinator.com/item?id=33116310
* Don't add obvious comments that can be inferred from just reading the code;
instead, use comments to explain why you're doing something, not what the
code is doing. Sometimes you can even extract some piece of code as a separate
function and name the function in such a way that it explains what the code
is doing and therefore there's no need to add a comment anymore.
* Add comments on top of code (on a line above it), not next to it, to avoid
horizontal scrolling.
* Do not commit commented code; if the code is commented it's better that it
gets removed. If there's a reason for why the commented piece of code should
not be removed, then write the reason why, in a comment on top of it. Otherwise
it's extremely confusing for the next developer (which could be your future
you) to find code that is commented/disabled.
* Our naming conventions are as follows:
    * Script names (e.g. files with `.sh`, `.bat` or `.fsx` extensions): snake_case.
    * CI job names: kebab-case.
    * Important public constants: TRAIN_CASE.
    * .NET (F# and C#) source files and projects (project names and project file names): PascalCase. An exception to this rule is project names and project file names that correspond to a console project which 
ends up being compiled and packaged in NuGet as a dotnet tool, for example: https://github.com/nblockchain/fsx/tree/master/fsxc
    * .NET APIs: PascalCase (see our [F# Style Guide](FSharpStyleGuide.md) for more info).
    * .NET parameters, local variables & nested functions: camelCase (again, see our [F# Style Guide](FSharpStyleGuide.md) for more info).
    * Please use verbs (in infinitive) for method/function & script names, and nouns for variables/constants/parameters/properties/fields.

* Be mindful of the way we prefer to use git:
    * When rebasing a PR, never use the command `git merge` or the GitHubUI to update branches, otherwise you might get some "Merge" commits that make your commit history unreadable.
    * Merge commits are only acceptable when caused by merging a PR that has more than 1 commit (if PR only has one commit, it's better to merge it with the "Rebase" option that GitHub provides).
    * Don't confuse the command `git rebase -i HEAD~n` (where `-i` stands for 'interactive') with the non-interactive `git rebase` operation.
        * The former is for squashing commits or changing their order. By the way, given that you need to be extremely careful when determining the number of commits `n` (if you put a number that is too high, even off by one, you might destroy previous merge commits), then we recommend to always add the `-r` flag when using the interactive mode (which is the shorthand for --rebase-merges). In conclusion: when you're about to use `git rebase -i HEAD~n`, rather always use `git rebase -ir HEAD~n`.
        * The latter is for rebasing a branch or PR against, for example, `master` or `main`, but only if there have been no recent force-pushes in the branch you want to rebase against. If there have been force-pushes, then the safest way to rebase is by using the `git cherry-pick` command.
    * When using the command `git pull`, never ever ever forget the flag `--rebase`, otherwise git will introduce merge commits for you automatically.

* Git commit messages should follow this style:

```
Scope/sub-scope: short title of what is changed (50 chars max)

Explanation of **why** (and maybe **how** as well, in case there's a part of
the change that is not self-explanatory). Don't hesitate to be very verbose
here, adding any references you may need, in this way[1], or even @nicknames of
people that helped. Manually crop your lines to not be longer than 64 chars.

Fixes https://github.com/nblockchain/geewallet/issues/45

[1] http://foo.bar/baz
```

As you can see, writing a commit message is generally like writing an e-mail: it
has a title at the top which is normally a short sentence (but not ended with a
dot, like most titles), and a body that starts in the 3rd line and which contains
one or many paragraphs (each ending with a dot, as it's text in prose). In
particular, the example above would be for a commit message that fixes the
issue #45. **Scope** usually refers to the project name, but without the need
to include the name of the project (e.g. in geewallet, all project names start
with the `GWallet.` prefix, then there's no need to specify it; so use `Backend`
as scope instead of `GWallet.Backend`). The **Sub-scope** may refer to a folder
or module inside what's represented as the scope, but it's not a strict mapping.

When referencing a bug/issue, as you can see above you can add a sentence at the
end of the commit message which starts with `Fixes `, followed by the **full URL**
of the bug/issue; this way, the ticket will be closed when the commit lands in
the main branch (which in some repos will be called `master`, or `main`). If the
commit is not really a fix for the issue, but you still want the ticket to be
closed after the commit lands, then you would use the word `Closes` instead of
`Fixes`.

Do not use long lines (manually crop them with EOLs because git doesn't do this
automatically).

For more info about why well-crafted commit messages are important, PLEASE PLEASE
read this article: https://web.archive.org/web/20240201135044/https://dhwthompson.com/2019/my-favourite-git-commit
