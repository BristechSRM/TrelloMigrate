module Config

open System
open System.Configuration

let private getConfigValue (key : string) = 
    let value = ConfigurationManager.AppSettings.Item(key)
    if String.IsNullOrWhiteSpace value then
        failwith <| sprintf "Configuration value with key %s is missing / is null  / is blank in configuration. Add key and value to proceed." key
    else 
        value

let private getUriConfigValue (key : string) = 
    let value = getConfigValue key
    Uri(value)

let srmOutputPath = getConfigValue "SrmOutputPath"
let sessionsServiceUri = getUriConfigValue "SessionsServiceUrl"
