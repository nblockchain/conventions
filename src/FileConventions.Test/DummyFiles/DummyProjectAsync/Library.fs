namespace DummyProjectAsync

module Say =

    let delayedHello name =
        async {
            do! Async.Sleep(5000)
        } |> Async.RunSynchronously
        "Delayed Hello"
