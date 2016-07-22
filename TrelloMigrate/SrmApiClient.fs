module SrmApiClient

open Config
open System
open SrmApiModels
open JsonHttpClient

let private parseQuotedGuid (guidString : string) = 
    Guid.Parse(guidString.Replace("\"",""))

module Profile = 
    let endpoint = Uri(sessionsServiceUri, "Profiles")

    let postAndGetId (profile : Profile) = 
        let result = post endpoint profile
        { profile with Id = parseQuotedGuid result }

module Handle = 
    let endpoint = Uri(sessionsServiceUri, "Handles")

    let post (handle : Handle) = 
        post endpoint handle |> ignore

module Session = 
    let endpoint = Uri(sessionsServiceUri, "Sessions")

    let postAndGetId (session : Session) = 
        let result = post endpoint session
        { session with Id = parseQuotedGuid result } 