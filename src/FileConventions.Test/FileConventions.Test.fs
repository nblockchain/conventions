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
    Assert.IsFalse(HasCorrectShebang(fileInfo))


[<Test>]
let HasCorrectShebangTest2 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "DummyWithShebang.fsx")))
    Assert.IsTrue(HasCorrectShebang(fileInfo))


[<Test>]
let HasCorrectShebangTest3 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "DummyWithWrongShebang.fsx")))
    Assert.IsFalse(HasCorrectShebang(fileInfo))


[<Test>]
let HasCorrectShebangTest4() =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "DummyEmpty.fsx")))
    Assert.IsFalse(HasCorrectShebang(fileInfo))


[<Test>]
let IsExecutableTest1 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "DummyExecutable.fsx")))
    Assert.IsTrue(IsExecutable(fileInfo))

[<Test>]
let IsExecutableTest2 () =
    let fileInfo = (FileInfo (Path.Combine(__SOURCE_DIRECTORY__, "DummyFiles", "DummyNotExecutable.fs")))
    Assert.IsFalse(IsExecutable(fileInfo))
