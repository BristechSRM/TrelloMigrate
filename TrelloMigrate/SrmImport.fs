module SrmImport

open SrmApiModels
open SrmApiClient

//Since everything in here is causing side effects, this module isn't as functional.
//Maybe this should be written in a imperative way instead to make that obvious? 

let private importProfileAndUpdateIds (profile : ProfileWithHandles) = 
    let updatedProfile = Profile.postAndGetId profile.Profile
    { Profile = updatedProfile
      Handles = profile.Handles |> Array.map (fun handle -> { handle with ProfileId = updatedProfile.Id }) } 

let private importAdmins (admins : Admin []) = 
    admins |> Array.map (fun admin -> { admin with ProfileWithHandles = importProfileAndUpdateIds admin.ProfileWithHandles })

let private importSpeakers (sessionsAndSpeakers : SessionSpeakerAndTrelloIds [] ) (adminProfiles : Admin []) = 

    let importSpeakerOrUseAdmin (speaker : ProfileWithHandles) =
        let foundAdmin = adminProfiles |> Array.tryFind (fun admin -> admin.ProfileWithHandles.Profile.Forename = speaker.Profile.Forename && admin.ProfileWithHandles.Profile.Surname = speaker.Profile.Surname) 
        match foundAdmin with
        | Some admin -> admin.ProfileWithHandles //TODO perform a merge to make sure no information is lost. 
        | None -> importProfileAndUpdateIds speaker

    sessionsAndSpeakers
    |> Array.map (fun ss -> 
        let updatedSpeaker = importSpeakerOrUseAdmin ss.Speaker
        let updatedSession = {ss.Session with SpeakerId = updatedSpeaker.Profile.Id}
        {ss with Speaker = updatedSpeaker; Session = updatedSession})        

let private importHandles (profiles : ProfileWithHandles []) = 
    profiles |> Array.iter (fun profile -> profile.Handles |> Array.iter Handle.post)

let private setAdminIdOnSessions (importedAdmins : Admin []) (ss : SessionSpeakerAndTrelloIds) = 
    let foundAdminId = 
        match ss.AdminTrelloId with 
        | Some trelloId -> 
            importedAdmins 
            |> Array.tryPick (fun admin -> if trelloId = admin.TrelloMemberId then Some admin.ProfileWithHandles.Profile.Id else None)
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
    importHandles (importedAdmins |> Array.map (fun x -> x.ProfileWithHandles))
    importHandles (importedSpeakersWithSessions |> Array.map (fun x -> x.Speaker))

    let preparedSessions = importedSpeakersWithSessions |> Array.map (setAdminIdOnSessions importedAdmins)
    let importedSessions = importSessions preparedSessions
    ()
