// printf not accepted here
namespace DummyProjectWithWrongConsole

module Say =
    let hello name =
        printfn "Hello %s" name
