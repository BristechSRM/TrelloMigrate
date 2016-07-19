module JsonHttpClient

open System
open System.Net
open System.Net.Http
open Newtonsoft.Json

let get<'Model> (uri : Uri) = 
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