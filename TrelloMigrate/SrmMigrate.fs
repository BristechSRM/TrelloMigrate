module SrmMigrate

open SrmApiClient
open SrmApiModels

//Since everything in here is causing side effects, this module isn't as functional.
//Maybe this should be written in a imperative way instead to make that obvious? 

let private migrateProfileAndUpdateIds (profile : ProfileWithHandles) = 
    let updatedProfile = Profile.postAndGetId profile.Profile
    { Profile = updatedProfile
      Handles = profile.Handles |> Array.map (fun handle -> { handle with ProfileId = updatedProfile.Id }) } 

let private migrateProfiles (profiles : Map<string, ProfileWithHandles>) = profiles |> Map.map (fun _ profile -> migrateProfileAndUpdateIds profile)     

let private migrateHandlesPerProfile (profiles : Map<string, ProfileWithHandles>) = 
    profiles
    |> Map.toArray
    |> Array.iter (fun (_, profile) -> profile.Handles |> Array.iter Handle.post)

let private setSpeakerAndAdminIdsOnSession (importedProfiles : Map<string, ProfileWithHandles>) (sr : SessionAndReferences) = 
    let foundSpeakerId = importedProfiles.[sr.SpeakerTrelloId].Profile.Id
    
    let foundAdminId = 
        match sr.AdminTrelloId with
        | Some trelloId -> Some importedProfiles.[trelloId].Profile.Id
        | None -> None
    
    let status = 
        match foundAdminId with
        | Some _ -> "assigned"
        | None -> "unassigned"
    
    { sr.Session with SpeakerId = foundSpeakerId
                      AdminId = foundAdminId
                      Status = status }
    
let private migrateSessions sessions = sessions |> Array.iter Session.post

let private setSenderAndReceiverOnCorrespondence (importedProfiles : Map<string,ProfileWithHandles>) (cr : CorrespondenceWithReferences) =
    { cr.Item with SenderId = importedProfiles.[cr.SenderTrelloId].Profile.Id
                   ReceiverId = importedProfiles.[cr.ReceiverTrelloId].Profile.Id }

let private migrateCorrespondence correspondence = 
    correspondence |> Array.iter Correspondence.post

let migrateAll (wrapper : SrmWrapper) = 
    printfn "Migrating all data"
    let importedProfiles = migrateProfiles wrapper.Profiles
    migrateHandlesPerProfile importedProfiles
    let preparedSessions = wrapper.Sessions |> Array.map (setSpeakerAndAdminIdsOnSession importedProfiles)
    migrateSessions preparedSessions
    let preparedCorrespondence = wrapper.Correspondence |> Array.map (setSenderAndReceiverOnCorrespondence importedProfiles)
    migrateCorrespondence preparedCorrespondence
    ()
