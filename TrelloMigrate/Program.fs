open System
open System.IO
open System.Net
open System.Net.Http
open Newtonsoft.Json
open TrelloModels
open SrmApiModels
open Credentials

module JsonHttpClient = 
    
    let get<'Model> (uri: Uri) = 
        use client = new HttpClient()
        let result = client.GetAsync(uri).Result
        match result.StatusCode with
        | HttpStatusCode.OK -> 
            let json = result.Content.ReadAsStringAsync().Result
            JsonConvert.DeserializeObject<'Model>(json)
        | errorCode -> 
            let errorMessage = result.Content.ReadAsStringAsync().Result
            let modelName = typeof<'Model>.Name
            failwith <| sprintf "Error in get request for %s. Status code: %i. Reason phrase: %s. Error Message: %s" modelName (int (errorCode)) result.ReasonPhrase errorMessage

module TrelloClient = 
    //Note: If number of Cards get higher than 1000, then paging needs to be implemented. 
    let getBasicCards trelloCred = 
        sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/cards/open?fields=id,name,idMembers,due&key=%s&token=%s" trelloCred.Key trelloCred.Token
        |> Uri
        |> JsonHttpClient.get<BasicCard []>

    let getBasicMembers trelloCred = 
        sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/members?fields=id,username,fullName,avatarHash&key=%s&token=%s" trelloCred.Key trelloCred.Token
        |> Uri
        |> JsonHttpClient.get<BasicMember []>

    let getBoardSummary trelloCred = 
        { BasicCards = getBasicCards trelloCred
          BasicMembers = getBasicMembers trelloCred }

module Transform = 
    let memberToProfile (bMember : BasicMember) : Profile = 
        { Id = Guid.Empty 
          Forename = bMember.FullName
          Surname = bMember.FullName
          Rating = 1 
          ImageUrl = String.Empty
          Bio = String.Empty }

    let toSrmOutline (board : BoardSummary) = 
        { Profiles = board.BasicMembers |> Array.map memberToProfile }        

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
