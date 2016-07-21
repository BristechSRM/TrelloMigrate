module SrmImport

open SrmApiModels
open SrmApiClient

//Since everything in here is causing side effects, this module isn't as functional.
//Maybe this should be written in a imperative way instead to make that obvious? 

let private importAdmins (wrapper : SrmWrapper) = 
    wrapper.Admins |> Array.map (fun admin -> 
        let updatedAdmin = Profile.postAndGetId admin.Profile
        { Profile = updatedAdmin
          Handles = admin.Handles |> Array.map (fun handle -> { handle with ProfileId = updatedAdmin.Id }) }) 

let private importSpeakers (wrapper : SrmWrapper) = 
    let speakerIsAdmin (admins : ProfileWithHandles []) (speaker : ProfileWithHandles) =
        let foundAdmin = admins |> Array.tryFind (fun admin -> admin.Profile.Forename = speaker.Profile.Forename && admin.Profile.Surname = speaker.Profile.Surname) 
        match foundAdmin with
        | Some _ -> true
        | None -> false
        
    wrapper.Speakers
    |> Array.filter (speakerIsAdmin wrapper.Admins >> not)
    |> Array.map (fun speaker -> 
        let updatedSpeaker = Profile.postAndGetId speaker.Profile
        { Profile = updatedSpeaker
          Handles = speaker.Handles |> Array.map (fun handle -> { handle with ProfileId = updatedSpeaker.Id }) })

let private importHandles (profiles : ProfileWithHandles []) = 
    profiles 
    |> Array.iter (fun profile -> 
        profile.Handles |> Array.iter Handle.post )
    
let importAll wrapper = 
    let importedAdmins = importAdmins wrapper
    let importedSpeakers = importSpeakers wrapper
    importHandles importedAdmins
    importHandles importedSpeakers
    
    ()
