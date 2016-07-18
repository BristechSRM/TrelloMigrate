module Credentials

open System
open System.Configuration

type TrelloApiCredentials = 
    { Key : string
      Token : string }

let getTrelloCredentials() = 
    let creds = 
        { Key = ConfigurationManager.AppSettings.Item("TrelloKey")
          Token = ConfigurationManager.AppSettings.Item("TrelloToken") }

    if String.IsNullOrWhiteSpace creds.Key || String.IsNullOrWhiteSpace creds.Token then 
        failwith 
            "TrelloKey or TrelloToken is missing. Use the TrelloCredsTemplate.config template to create a TrelloCreds.config, and add the values for your Trello Api Key and Token"
    else creds
