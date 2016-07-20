module SrmImport

open SrmApiModels
open SrmApiClient

//Since everything in here is causing side effects, this module isn't as functional.
//Maybe this should be written in a imperative way instead to make that obvious? 

let private importAdmins (wrapper : SrmWrapper) = Array.map Profile.postAndGetId wrapper.Admins

let private importSpeakers (wrapper : SrmWrapper) = 
    let speakerIsAdmin (admins : Profile []) (speaker : Profile) =
        let foundAdmin = admins |> Array.tryFind (fun admin -> admin.Forename = speaker.Forename && admin.Surname = speaker.Surname) 
        match foundAdmin with
        | Some _ -> true
        | None -> false
        
    wrapper.Speakers
    |> Array.filter (speakerIsAdmin wrapper.Admins)
    |> Array.map Profile.postAndGetId
    
let importAll wrapper = 
    let importedAdmins = importAdmins wrapper
    let importedSpeakers = importSpeakers wrapper
    ()
