open System.IO
open Newtonsoft.Json

[<EntryPoint>]
let main argv = 
    let saveData path data = 
        let result = JsonConvert.SerializeObject(data, Formatting.Indented)
        File.WriteAllText(path, result)
    try 
        JsonSettings.setDefaults()
        let srmOutputFile = @"SrmApiModelsPreImport.json"
        Credentials.getTrelloCredentials()
        |> TrelloClient.getBoardSummary
        |> Transform.toSrmOutline
        |> saveData srmOutputFile
        0
    with ex -> 
        printfn "%A" ex
        1
