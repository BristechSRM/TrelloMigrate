module JsonHttpClient

open System
open System.Net
open System.Net.Http
open Newtonsoft.Json
open System.Text

let get<'Model> (uri : Uri) = 
    use client = new HttpClient()
    let response = client.GetAsync(uri).Result
    match response.StatusCode with
    | HttpStatusCode.OK -> 
        let json = response.Content.ReadAsStringAsync().Result
        JsonConvert.DeserializeObject<'Model>(json)
    | errorCode -> 
        let errorMessage = response.Content.ReadAsStringAsync().Result
        let modelName = typeof<'Model>.Name
        failwith <| sprintf "Error in get request for %s. Status code: %i. Reason phrase: %s. Error Message: %s" modelName (int (errorCode)) response.ReasonPhrase errorMessage

let post (uri : Uri) (data : 'Model) = 
    use client = new HttpClient()
    let jsonData = JsonConvert.SerializeObject(data)
    let content = new StringContent(jsonData, Encoding.UTF8, "application/json")
    let response = client.PostAsync(uri, content).Result
    match response.StatusCode with
    | HttpStatusCode.Created -> response.Content.ReadAsStringAsync().Result
    | errorCode -> 
        let errorMessage = response.Content.ReadAsStringAsync().Result
        let modelName = typeof<'Model>.Name
        failwith <| sprintf "Error in post request for %s. Status code: %i. Reason phrase: %s. Error Message: %s" modelName (int (errorCode)) response.ReasonPhrase errorMessage

