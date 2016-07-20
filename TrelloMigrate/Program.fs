open System.IO
open Newtonsoft.Json

[<EntryPoint>]
let main argv = 
    let saveData path data = 
        let result = JsonConvert.SerializeObject(data, Formatting.Indented)
        File.WriteAllText(path, result)
    try 
        JsonSettings.setDefaults()
        Credentials.getTrelloCredentials()
        |> TrelloClient.getBoardSummary
        |> Transform.toSrmModels
        |> (fun wrapper -> saveData Config.srmOutputPath wrapper; wrapper)
        |> SrmImport.importAll

        0
    with ex -> 
        printfn "%A" ex
        1
