module FileConventions.Test

open NUnit.Framework

open FileConventions

[<SetUp>]
let Setup () =
    ()

[<Test>]
let HasBinaryContentTest1 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "FileConventions.Test.pdb")))
    Assert.That(HasCorrectShebang fileInfo, Is.EqualTo true)
