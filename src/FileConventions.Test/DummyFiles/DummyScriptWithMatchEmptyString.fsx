let temp = "Hello World"

match temp with
| "" -> failwith "Empty String"
| _ -> failwith "Non-Empty String"
