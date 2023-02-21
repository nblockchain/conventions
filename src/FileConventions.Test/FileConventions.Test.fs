module FileConventions.Test

open System.IO

open NUnit.Framework

open FileConventions

[<SetUp>]
let Setup () =
    ()

[<Test>]
let HasBinaryContentTest1 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "FileConventions.Test.pdb")))
    Assert.That(HasBinaryContent fileInfo, Is.EqualTo true)
