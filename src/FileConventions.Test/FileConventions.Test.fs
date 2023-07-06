module FileConventions.Test

open System
open System.IO

open NUnit.Framework
open NUnit.Framework.Constraints

open FileConventions

open type FileConventions.EolAtEof

[<SetUp>]
let Setup() =
    ()

let dummyFilesDirectory =
    DirectoryInfo(Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles"))

[<Test>]
let HasCorrectShebangTest1() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyWithoutShebang.fsx"
            )
        ))

    Assert.That(HasCorrectShebang fileInfo, Is.EqualTo false)


[<Test>]
let HasCorrectShebangTest2() =
    let fileInfo =
        (FileInfo(
            Path.Combine(dummyFilesDirectory.FullName, "DummyWithShebang.fsx")
        ))

    Assert.That(HasCorrectShebang fileInfo, Is.EqualTo true)


[<Test>]
let HasCorrectShebangTest3() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyWithWrongShebang.fsx"
            )
        ))

    Assert.That(HasCorrectShebang fileInfo, Is.EqualTo false)


[<Test>]
let HasCorrectShebangTest4() =
    let fileInfo =
        (FileInfo(Path.Combine(dummyFilesDirectory.FullName, "DummyEmpty.fsx")))

    Assert.That(HasCorrectShebang fileInfo, Is.EqualTo false)


[<Test>]
let MixedLineEndingsTest1() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyWithMixedLineEndings"
            )
        ))

    Assert.That(MixedLineEndings fileInfo, Is.EqualTo true)


[<Test>]
let MixedLineEndingsTest2() =
    let fileInfo =
        (FileInfo(
            Path.Combine(dummyFilesDirectory.FullName, "DummyWithLFLineEndings")
        ))

    Assert.That(MixedLineEndings fileInfo, Is.EqualTo false)


[<Test>]
let MixedLineEndingsTest3() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyWithCRLFLineEndings"
            )
        ))

    Assert.That(MixedLineEndings fileInfo, Is.EqualTo false)


[<Test>]
let DetectUnpinnedVersionsInGitHubCI1() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyCIWithLatestTag.yml"
            )
        ))

    Assert.That(DetectUnpinnedVersionsInGitHubCI fileInfo, Is.EqualTo true)


[<Test>]
let DetectUnpinnedVersionsInGitHubCI2() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyCIWithoutLatestTag.yml"
            )
        ))

    Assert.That(DetectUnpinnedVersionsInGitHubCI fileInfo, Is.EqualTo false)


[<Test>]
let DetectUnpinnedDotnetToolInstallVersions1() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyCIWithUnpinnedDotnetToolInstallVersion.yml"
            )
        ))

    Assert.That(
        DetectUnpinnedDotnetToolInstallVersions fileInfo,
        Is.EqualTo true
    )

[<Test>]
let DetectUnpinnedDotnetToolInstallVersions2() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyCIWithoutUnpinnedDotnetToolInstallVersion.yml"
            )
        ))

    Assert.That(
        DetectUnpinnedDotnetToolInstallVersions fileInfo,
        Is.EqualTo false
    )


[<Test>]
let DetectUnpinnedNpmPackageInstallVersions1() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyCIWithUnpinnedNpmPackageInstallVersion1.yml"
            )
        ))

    Assert.That(
        DetectUnpinnedNpmPackageInstallVersions fileInfo,
        Is.EqualTo true
    )


[<Test>]
let DetectUnpinnedNpmPackageInstallVersions2() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyCIWithoutUnpinnedNpmPackageInstallVersion.yml"
            )
        ))

    Assert.That(
        DetectUnpinnedNpmPackageInstallVersions fileInfo,
        Is.EqualTo false
    )


[<Test>]
let DetectUnpinnedNpmPackageInstallVersions3() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyCIWithUnpinnedNpmPackageInstallVersion2.yml"
            )
        ))

    Assert.That(
        DetectUnpinnedNpmPackageInstallVersions fileInfo,
        Is.EqualTo true
    )


[<Test>]
let DetectUnpinnedNpmPackageInstallVersions4() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyCIWithUnpinnedNpmPackageInstallVersion3.yml"
            )
        ))

    Assert.That(
        DetectUnpinnedNpmPackageInstallVersions fileInfo,
        Is.EqualTo true
    )


[<Test>]
let DetectAsteriskInPackageReferenceItems1() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyFsprojWithAsterisk.fsproj"
            )
        ))

    Assert.That(DetectAsteriskInPackageReferenceItems fileInfo, Is.EqualTo true)


[<Test>]
let DetectAsteriskInPackageReferenceItems2() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyFsprojWithoutAsterisk.fsproj"
            )
        ))

    Assert.That(
        DetectAsteriskInPackageReferenceItems fileInfo,
        Is.EqualTo false
    )


[<Test>]
let MissingVersionsInNugetPackageReferencesTest1() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyWithMissingVersionsInNugetPackageReferences.fsx"
            )
        ))

    Assert.That(
        DetectMissingVersionsInNugetPackageReferences fileInfo,
        Is.EqualTo true
    )


[<Test>]
let MissingVersionsInNugetPackageReferencesTest2() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyWithoutMissingVersionsInNugetPackageReferences.fsx"
            )
        ))

    Assert.That(
        DetectMissingVersionsInNugetPackageReferences fileInfo,
        Is.EqualTo false
    )


[<Test>]
let MissingVersionsInNugetPackageReferencesTest3() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyWithoutNugetPackageReferences.fsx"
            )
        ))

    Assert.That(
        DetectMissingVersionsInNugetPackageReferences fileInfo,
        Is.EqualTo false
    )


[<Test>]
let EolAtEofTest1() =
    let fileInfo =
        (FileInfo(
            Path.Combine(dummyFilesDirectory.FullName, "DummyWithEolAtEof.txt")
        ))

    Assert.That(EolAtEof fileInfo, Is.EqualTo True)


[<Test>]
let EolAtEofTest2() =
    let fileInfo =
        (FileInfo(
            Path.Combine(
                dummyFilesDirectory.FullName,
                "DummyWithoutEolAtEof.txt"
            )
        ))

    Assert.That(EolAtEof fileInfo, Is.EqualTo False)


[<Test>]
let EolAtEofTest3() =
    let fileInfo =
        (FileInfo(Path.Combine(dummyFilesDirectory.FullName, "someLib.dll")))

    Assert.That(EolAtEof fileInfo, Is.EqualTo NotApplicable)


[<Test>]
let HasBinaryContentTest1() =
    let fileInfo =
        (FileInfo(Path.Combine(dummyFilesDirectory.FullName, "someLib.dll")))

    Assert.That(HasBinaryContent fileInfo, Is.EqualTo true)


[<Test>]
let HasBinaryContentTest2() =
    let fileInfo =
        (FileInfo(Path.Combine(dummyFilesDirectory.FullName, "white.png")))

    Assert.That(HasBinaryContent fileInfo, Is.EqualTo true)


[<Test>]
let HasBinaryContentTest3() =
    let fileInfo =
        (FileInfo(
            Path.Combine(dummyFilesDirectory.FullName, "DummyNonBinaryFile.txt")
        ))

    Assert.That(HasBinaryContent fileInfo, Is.EqualTo false)


[<Test>]
let WrapTextTest1() =
    let characterCount = 64

    let paragraph =
        "This is a very very very very long line with more than 64 characters."

    let expectedResult =
        "This is a very very very very long line with more than 64"
        + Environment.NewLine
        + "characters."

    Assert.That(WrapText paragraph characterCount, Is.EqualTo expectedResult)

[<Test>]
let WrapTextTest2() =
    let characterCount = 64

    let paragraph =
        "This is short line."
        + Environment.NewLine
        + "```"
        + Environment.NewLine
        + "This is a very very very very long line with more than 64 characters inside a code block."
        + Environment.NewLine
        + "```"

    let expectedResult = paragraph

    Assert.That(WrapText paragraph characterCount, Is.EqualTo expectedResult)

[<Test>]
let WrapTextTest3() =
    let characterCount = 64
    let tenDigits = "1234567890"

    let seventyChars =
        tenDigits
        + tenDigits
        + tenDigits
        + tenDigits
        + tenDigits
        + tenDigits
        + tenDigits

    let paragraph =
        "This is short line referring to [1]."
        + Environment.NewLine
        + "[1] someUrl://"
        + seventyChars

    let expectedResult = paragraph

    Assert.That(WrapText paragraph characterCount, Is.EqualTo expectedResult)


[<Test>]
let WrapTextTest4() =
    let characterCount = 64

    let text =
        "This is short line."
        + Environment.NewLine
        + Environment.NewLine
        + "This is a very very very very very long line with more than 64 characters."

    let expectedResult =
        "This is short line."
        + Environment.NewLine
        + Environment.NewLine
        + "This is a very very very very very long line with more than 64"
        + Environment.NewLine
        + "characters."

    Assert.That(WrapText text characterCount, Is.EqualTo expectedResult)

[<Test>]

let DetectInconsistentVersionsInGitHubCIWorkflow1() =
    let fileInfo =
        (Seq.singleton(
            FileInfo(
                Path.Combine(
                    dummyFilesDirectory.FullName,
                    "DummyCIWithSamePulumiVersion.yml"
                )
            )
        ))

    Assert.That(
        DetectInconsistentVersionsInGitHubCIWorkflow fileInfo,
        Is.EqualTo false
    )


[<Test>]
let DetectInconsistentVersionsInGitHubCIWorkflow2() =
    let fileInfo =
        (Seq.singleton(
            FileInfo(
                Path.Combine(
                    dummyFilesDirectory.FullName,
                    "DummyCIWithoutSamePulumiVersion.yml"
                )
            )
        ))

    Assert.That(
        DetectInconsistentVersionsInGitHubCIWorkflow fileInfo,
        Is.EqualTo true
    )


[<Test>]
let DetectInconsistentVersionsInGitHubCIWorkflow3() =
    let fileInfo =
        (Seq.singleton(
            FileInfo(
                Path.Combine(
                    dummyFilesDirectory.FullName,
                    "DummyCIWithoutSameSetupPulumiVersion.yml"
                )
            )
        ))

    Assert.That(
        DetectInconsistentVersionsInGitHubCIWorkflow fileInfo,
        Is.EqualTo true
    )


[<Test>]
let DetectInconsistentVersionsInGitHubCIWorkflow4() =
    let fileInfo =
        (Seq.singleton(
            FileInfo(
                Path.Combine(
                    dummyFilesDirectory.FullName,
                    "DummyCIWithSameSetupPulumiVersion.yml"
                )
            )
        ))

    Assert.That(
        DetectInconsistentVersionsInGitHubCIWorkflow fileInfo,
        Is.EqualTo false
    )


[<Test>]
let DetectInconsistentVersionsInGitHubCIWorkflow5() =
    let fileInfo =
        (seq {

            FileInfo(
                Path.Combine(
                    dummyFilesDirectory.FullName,
                    "DummyCIWithSetupPulumiVersionV2.0.0.yml"
                )
            )

            FileInfo(
                Path.Combine(
                    dummyFilesDirectory.FullName,
                    "DummyCIWithSetupPulumiVersionV2.0.1.yml"
                )
            )

        })

    Assert.That(
        DetectInconsistentVersionsInGitHubCIWorkflow fileInfo,
        Is.EqualTo true
    )


[<Test>]
let DetectInconsistentVersionsInGitHubCIWorkflow6() =
    let fileInfo =
        (Seq.singleton(
            FileInfo(
                Path.Combine(
                    dummyFilesDirectory.FullName,
                    "DummyCIWithoutSameCheckoutVersion.yml"
                )
            )
        ))

    Assert.That(
        DetectInconsistentVersionsInGitHubCIWorkflow fileInfo,
        Is.EqualTo true
    )


[<Test>]
let DetectInconsistentVersionsInGitHubCIWorkflow7() =
    let fileInfo =
        (Seq.singleton(
            FileInfo(
                Path.Combine(
                    dummyFilesDirectory.FullName,
                    "DummyCIWithoutSameNodeVersion.yml"
                )
            )
        ))

    Assert.That(
        DetectInconsistentVersionsInGitHubCIWorkflow fileInfo,
        Is.EqualTo true
    )


[<Test>]
let DetectInconsistentVersionsInGitHubCI1() =
    let fileInfo =
        DirectoryInfo(
            Path.Combine(dummyFilesDirectory.FullName, "DummyWorkflows")
        )

    Assert.That(DetectInconsistentVersionsInGitHubCI fileInfo, Is.EqualTo true)


[<Test>]
let DetectInconsistentVersionsInNugetRefsInFSharpScripts1() =
    let fileInfos =
        (seq {

            FileInfo(
                Path.Combine(
                    dummyFilesDirectory.FullName,
                    "DummyFsharpScriptWithFsdkVersion0.6.0.fsx"
                )
            )

            FileInfo(
                Path.Combine(
                    dummyFilesDirectory.FullName,
                    "DummyFsharpScriptWithFsdkVersion0.6.1.fsx"
                )
            )

        })

    Assert.That(
        DetectInconsistentVersionsInNugetRefsInFSharpScripts fileInfos,
        Is.EqualTo true
    )

[<Test>]
let DetectInconsistentVersionsInFSharpScripts1() =
    let fileInfo =
        DirectoryInfo(
            Path.Combine(dummyFilesDirectory.FullName, "DummyScripts")
        )

    Assert.That(
        DetectInconsistentVersionsInFSharpScripts fileInfo None,
        Is.EqualTo true
    )


[<Test>]
let DetectInconsistentVersionsInFSharpScripts2() =
    let dir =
        DirectoryInfo(
            Path.Combine(dummyFilesDirectory.FullName, "DummyScripts")
        )

    Assert.That(
        DetectInconsistentVersionsInFSharpScripts
            dir
            (Some(Seq.singleton "DummyScripts")),
        Is.EqualTo false
    )
