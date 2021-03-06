﻿open Newtonsoft.Json
open System.IO

[<EntryPoint>]
let main _ = 
    let saveData path data = 
        let result = JsonConvert.SerializeObject(data, Formatting.Indented)
        File.WriteAllText(path, result)
    try 
        JsonSettings.setDefaults()
        let preMigrationSrmModels = 
            Credentials.getTrelloCredentials()
            |> TrelloClient.getBoardSummary
            |> Transform.toSrmModels

        saveData Config.srmOutputPath preMigrationSrmModels
        
        SrmMigrate.migrateAll preMigrationSrmModels
        printfn "Migration Complete"
        0
    with ex -> 
        printfn "%A" ex
        1
