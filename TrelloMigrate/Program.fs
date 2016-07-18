open System
open System.Net
open System.Net.Http
open Newtonsoft.Json
open TrelloModels
open JsonSettings

[<EntryPoint>]
let main argv = 
    try 
        JsonSettings.setDefaults()
        let trelloCred = Credentials.getTrelloCredentials()
        let cardsUri = 
            Uri <| sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/cards/open?fields=id,name,idMembers,due&key=%s&token=%s" trelloCred.Key trelloCred.Token
        use client = new HttpClient()
        let result = client.GetAsync(cardsUri).Result
        match result.StatusCode with
        | HttpStatusCode.OK -> 
            let json = result.Content.ReadAsStringAsync().Result
            let basicCards = JsonConvert.DeserializeObject<BasicCard []>(json)
            printfn "%s" json
        | errorCode -> 
            let errorMessage = result.Content.ReadAsStringAsync().Result
            failwith <| sprintf "Error fetching cards from srm board. Status code: %i. Reason phrase: %s. Error Message: %s" (int (errorCode)) result.ReasonPhrase errorMessage
        0
    with ex -> 
        printfn "%A" ex
        1
