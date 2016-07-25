module SrmImport

open SrmApiModels
open SrmApiClient

//Since everything in here is causing side effects, this module isn't as functional.
//Maybe this should be written in a imperative way instead to make that obvious? 

let private importProfileAndUpdateIds (profile : ProfileWithHandles) = 
    let updatedProfile = Profile.postAndGetId profile.Profile
    { Profile = updatedProfile
      Handles = profile.Handles |> Array.map (fun handle -> { handle with ProfileId = updatedProfile.Id }) } 

let private importAdmins (admins : Map<string,ProfileWithHandles>) = 
    admins |> Map.map (fun _ admin -> importProfileAndUpdateIds admin)

let private importSpeakers (sessionsAndSpeakers : SessionSpeakerAndTrelloIds []) (admins : Map<string,ProfileWithHandles>) = 
//    let importSpeakerOrUseAdmin (speaker : ProfileWithHandles) = 
//        let foundAdmin = 
//            admins 
//            |> Map.tryFind 
//                   (fun admin -> admin.ProfileWithHandles.Profile.Forename = speaker.Profile.Forename && admin.ProfileWithHandles.Profile.Surname = speaker.Profile.Surname)
//        match foundAdmin with
//        | Some admin -> admin.ProfileWithHandles //TODO perform a merge to make sure no information is lost. 
//        | None -> importProfileAndUpdateIds speaker
    sessionsAndSpeakers |> Array.map (fun ss -> 
                               let updatedSpeaker = importProfileAndUpdateIds ss.Speaker
                               let updatedSession = { ss.Session with SpeakerId = updatedSpeaker.Profile.Id }
                               { ss with Speaker = updatedSpeaker
                                         Session = updatedSession })        

let private importHandles (profiles : ProfileWithHandles []) = 
    profiles |> Array.iter (fun profile -> profile.Handles |> Array.iter Handle.post)

let private setAdminIdOnSessions (importedAdmins : Map<string,ProfileWithHandles>) (ss : SessionSpeakerAndTrelloIds) = 
    let foundAdminId = 
        match ss.AdminTrelloId with 
        | Some trelloId -> Some importedAdmins.[trelloId].Profile.Id
        | None -> None
    let status = 
        match foundAdminId with
        | Some _ -> "assigned"
        | None -> "unassigned" 
    {ss with Session = {ss.Session with AdminId = foundAdminId; Status = status }}
    
let private importSessions sessionsAndSpeakers = 
    sessionsAndSpeakers |> Array.map (fun s -> Session.postAndGetId s.Session)

let importAll wrapper = 
    let importedAdmins = importAdmins wrapper.Admins
    let importedSpeakersWithSessions = importSpeakers wrapper.SessionsAndSpeakers importedAdmins
    importHandles (importedAdmins
                   |> Map.toArray
                   |> Array.map (fun (_, admin) -> admin))
    importHandles (importedSpeakersWithSessions |> Array.map (fun x -> x.Speaker))
    let preparedSessions = importedSpeakersWithSessions |> Array.map (setAdminIdOnSessions importedAdmins)
    let importedSessions = importSessions preparedSessions
    ()