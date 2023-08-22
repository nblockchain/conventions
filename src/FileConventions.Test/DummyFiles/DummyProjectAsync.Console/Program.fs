async { do! Async.Sleep(5000) } |> Async.RunSynchronously
printf "Hello World"
