module SrmImport

open SrmApiModels
open SrmApiClient

//Since everything in here is causing side effects, this module isn't as functional.
//Maybe this should be written in a imperative way instead to make that obvious? 

let private importAdmins (wrapper : SrmWrapper) = 
    wrapper.Admins |> Array.map (fun admin -> Profile.postAndGetId admin.Profile)

let private importSpeakers (wrapper : SrmWrapper) = 
    let speakerIsAdmin (admins : ProfileWithHandles []) (speaker : ProfileWithHandles) =
        let foundAdmin = admins |> Array.tryFind (fun admin -> admin.Profile.Forename = speaker.Profile.Forename && admin.Profile.Surname = speaker.Profile.Surname) 
        match foundAdmin with
        | Some _ -> true
        | None -> false
        
    wrapper.Speakers
    |> Array.filter (speakerIsAdmin wrapper.Admins >> not)
    |> Array.map (fun speaker -> Profile.postAndGetId speaker.Profile)
    
let importAll wrapper = 
    let importedAdmins = importAdmins wrapper
    let importedSpeakers = importSpeakers wrapper
    ()
