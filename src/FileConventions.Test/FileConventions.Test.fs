module FileConventions.Test

open System
open System.IO
open FileConventions
open NUnit.Framework
open NUnit.Framework.Constraints

[<SetUp>]
let Setup () =
    ()

[<Test>]
let HasCorrectShebangTest1 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "DummyWithoutShebang.fsx")))
    Assert.That(HasCorrectShebang fileInfo, Is.EqualTo false)


[<Test>]
let HasCorrectShebangTest2 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "DummyWithShebang.fsx")))
    Assert.That(HasCorrectShebang fileInfo, Is.EqualTo true)


[<Test>]
let HasCorrectShebangTest3 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "DummyWithWrongShebang.fsx")))
    Assert.That(HasCorrectShebang fileInfo, Is.EqualTo false)


[<Test>]
let HasCorrectShebangTest4() =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "DummyEmpty.fsx")))
    Assert.That(HasCorrectShebang fileInfo, Is.EqualTo false)


[<Test>]
let IsExecutableTest1 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "DummyExecutable.fsx")))
    Assert.That(IsExecutable fileInfo, Is.EqualTo true)


[<Test>]
let IsExecutableTest2 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "DummyNotExecutable.fs")))
    Assert.That(IsExecutable fileInfo, Is.EqualTo false)
