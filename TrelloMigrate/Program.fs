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
        |> Transform.toSrmSummary
        |> saveData Config.srmOutputPath
        0
    with ex -> 
        printfn "%A" ex
        1
