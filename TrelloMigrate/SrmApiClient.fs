module SrmApiClient

open Config
open System
open SrmApiModels
open JsonHttpClient

let private parseQuotedGuid (guidString : string) = Guid.Parse(guidString.Replace("\"",""))

let postAndGetGuid uri data = post uri data |> parseQuotedGuid

module Profile = 
    let private endpoint = Uri(sessionsServiceUri, "Profiles")

    let postAndGetId (profile : Profile) = 
        { profile with Id = postAndGetGuid endpoint profile }

module Handle = 
    let private endpoint = Uri(sessionsServiceUri, "Handles")

    let post (handle : Handle) = 
        post endpoint handle |> ignore

module Session = 
    let private endpoint = Uri(sessionsServiceUri, "Sessions")

    let postAndGetId (session : Session) = 
        { session with Id = postAndGetGuid endpoint session} 