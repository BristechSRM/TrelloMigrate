module SrmImport

open SrmApiModels
open SrmApiClient

//Since everything in here is causing side effects, this module isn't as functional.
//Maybe this should be written in a imperative way instead to make that obvious? 

let private importProfileAndUpdateIds (profile : ProfileWithHandles) = 
    let updatedProfile = Profile.postAndGetId profile.Profile
    { Profile = updatedProfile
      Handles = profile.Handles |> Array.map (fun handle -> { handle with ProfileId = updatedProfile.Id }) } 

let private importProfiles (profiles : Map<string,ProfileWithHandles>) = 
    profiles |> Map.map (fun _ profile -> importProfileAndUpdateIds profile)     

let private importHandlesPerProfile (profiles : Map<string,ProfileWithHandles>) = 
    profiles
    |> Map.toArray
    |> Array.iter (fun (_, profile) -> profile.Handles |> Array.iter Handle.post)

let private setSpeakerAndAdminIdsOnSession (importedProfiles : Map<string,ProfileWithHandles>) (ss : SessionAndReferences) =
    let foundSpeakerId = importedProfiles.[ss.SpeakerTrelloId].Profile.Id
    let foundAdminId = 
        match ss.AdminTrelloId with 
        | Some trelloId -> Some importedProfiles.[trelloId].Profile.Id
        | None -> None
    let status = 
        match foundAdminId with
        | Some _ -> "assigned"
        | None -> "unassigned" 

    {ss.Session with SpeakerId = foundSpeakerId; AdminId = foundAdminId; Status = status}
    
let private importSessions sessions = 
    sessions |> Array.map Session.postAndGetId

let importAll (wrapper: SrmWrapper) = 
    let importedProfiles = importProfiles wrapper.Profiles
    importHandlesPerProfile importedProfiles
    let preparedSessions = wrapper.Sessions |> Array.map (setSpeakerAndAdminIdsOnSession importedProfiles)
    let importedSessions = importSessions preparedSessions
    ()