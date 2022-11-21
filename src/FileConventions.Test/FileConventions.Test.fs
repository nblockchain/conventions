module FileConventions.Test

open System
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


[<Test>]
let HasBinaryContentTest2 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "FileConventions.Test.dll")))
    Assert.That(HasBinaryContent fileInfo, Is.EqualTo true)


[<Test>]
let HasBinaryContentTest3 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "libMono.Unix.a")))
    Assert.That(HasBinaryContent fileInfo, Is.EqualTo true)


[<Test>]
let HasBinaryContentTest4 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "libMono.Unix.dylib")))
    Assert.That(HasBinaryContent fileInfo, Is.EqualTo true)


[<Test>]
let HasBinaryContentTest5 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "libMono.Unix.so")))
    Assert.That(HasBinaryContent fileInfo, Is.EqualTo true)


[<Test>]
let HasBinaryContentTest6 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "white.jpeg")))
    Assert.That(HasBinaryContent fileInfo, Is.EqualTo true)


[<Test>]
let HasBinaryContentTest7 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "white.png")))
    Assert.That(HasBinaryContent fileInfo, Is.EqualTo true)


[<Test>]
let HasBinaryContentTest8 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "Program.fs")))
    Assert.That(HasBinaryContent fileInfo, Is.EqualTo false)


[<Test>]
let HasBinaryContentTest9 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "project.assets.json")))
    Assert.That(HasBinaryContent fileInfo, Is.EqualTo false)


[<Test>]
let HasBinaryContentTest10 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "project.nuget.cache")))
    Assert.That(HasBinaryContent fileInfo, Is.EqualTo false)


[<Test>]
let HasBinaryContentTest11 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "FileConventions.fsproj.nuget.g.targets")))
    Assert.That(HasBinaryContent fileInfo, Is.EqualTo false)


[<Test>]
let HasBinaryContentTest12 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "FileConventions.fsproj.nuget.g.props")))
    Assert.That(HasBinaryContent fileInfo, Is.EqualTo false)
