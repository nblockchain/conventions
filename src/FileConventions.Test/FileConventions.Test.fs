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
    let fileInfo = (FileInfo $"{__SOURCE_DIRECTORY__}{Path.DirectorySeparatorChar}DummyWithoutShebang.fsx")
    Assert.IsFalse(HasCorrectShebang(fileInfo))


[<Test>]
let HasCorrectShebangTest2 () =
    let fileInfo = (FileInfo $"{__SOURCE_DIRECTORY__}{Path.DirectorySeparatorChar}DummyWithShebang.fsx")
    Assert.IsTrue(HasCorrectShebang(fileInfo))


[<Test>]
let HasCorrectShebangTest3 () =
    let fileInfo = (FileInfo $"{__SOURCE_DIRECTORY__}{Path.DirectorySeparatorChar}DummyWithWrongShebang.fsx")
    Assert.IsFalse(HasCorrectShebang(fileInfo))


[<Test>]
let HasCorrectShebangTest4() =
    let fileInfo = (FileInfo $"{__SOURCE_DIRECTORY__}{Path.DirectorySeparatorChar}DummyEmpty.fsx")
    Assert.IsFalse(HasCorrectShebang(fileInfo))


[<Test>]
let IsExecutable () =
    let fileInfo = (FileInfo $"{__SOURCE_DIRECTORY__}{Path.DirectorySeparatorChar}DummyExecutable.fsx")
    Assert.Fail()
