﻿module SrmApiClient

open Config
open SrmApiModels
open System

let private parseQuotedGuid (guidString : string) = Guid.Parse(guidString.Replace("\"", ""))
let private postAndGetGuid uri data = JsonHttpClient.post uri data |> parseQuotedGuid
let private post uri data = JsonHttpClient.post uri data |> ignore

module Profile = 
    let private endpoint = Uri(sessionsServiceUri, "Profiles")
    let postAndGetId (profile : Profile) = { profile with Id = postAndGetGuid endpoint profile }

module Handle = 
    let private endpoint = Uri(sessionsServiceUri, "Handles")
    let post (handle : Handle) = post endpoint handle

module Session = 
    let private endpoint = Uri(sessionsServiceUri, "Sessions")
    let post (session : Session) = post endpoint session
