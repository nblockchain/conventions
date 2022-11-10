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
let HasShebangTest1 () =
    let fileInfo = (FileInfo $"{__SOURCE_DIRECTORY__}{Path.DirectorySeparatorChar}DummyWithoutShebang.fsx")
    Assert.IsFalse(HasShebang(fileInfo))


[<Test>]
let HasShebangTest2 () =
    let fileInfo = (FileInfo $"{__SOURCE_DIRECTORY__}{Path.DirectorySeparatorChar}DummyWithShebang.fsx")
    Assert.IsTrue(HasShebang(fileInfo))
