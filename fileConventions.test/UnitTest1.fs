module fileConventions.test
open System
open System.IO
open fileConventions
open NUnit.Framework
open NUnit.Framework.Constraints

[<SetUp>]
let Setup () =
    ()

[<Test>]
let Test1 () =
    // let fileInfo = (FileInfo $"{__SOURCE_DIRECTORY__}{Path.DirectorySeparatorChar}Dummy.fsx")
    // Assert.IsFalse(HasShebang(fileInfo))
    Assert.Fail()
