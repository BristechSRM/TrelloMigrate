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