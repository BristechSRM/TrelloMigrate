module SrmImport

open SrmApiModels
open SrmApiClient

//Since everything in here is causing side effects, this module isn't as functional.
//Maybe this should be written in a imperative way instead to make that obvious? 

let private importProfileAndUpdateIds (profile : ProfileWithHandles) = 
    let updatedProfile = Profile.postAndGetId profile.Profile
    { Profile = updatedProfile
      Handles = profile.Handles |> Array.map (fun handle -> { handle with ProfileId = updatedProfile.Id }) } 

let private importSpeakers (sessionsAndSpeakers : SessionAndSpeaker [] ) (adminProfiles : ProfileWithHandles []) = 

    let importSpeakerOrUseAdmin (speaker : ProfileWithHandles) =
        let foundAdmin = adminProfiles |> Array.tryFind (fun admin -> admin.Profile.Forename = speaker.Profile.Forename && admin.Profile.Surname = speaker.Profile.Surname) 
        match foundAdmin with
        | Some admin -> admin //TODO perform a merge to make sure no information is lost. 
        | None -> importProfileAndUpdateIds speaker

    sessionsAndSpeakers
    |> Array.map (fun ss -> 
        let updatedSpeaker = importSpeakerOrUseAdmin ss.Speaker
        let updatedSession = {ss.Session with SpeakerId = updatedSpeaker.Profile.Id}
        {ss with Speaker = updatedSpeaker; Session = updatedSession})        

let private importHandles (profiles : ProfileWithHandles []) = 
    profiles |> Array.iter (fun profile -> profile.Handles |> Array.iter Handle.post)
    
let importAll wrapper = 
    let importedAdmins = wrapper.Admins |> Array.map importProfileAndUpdateIds
    let importedSpeakersWithSessions = importSpeakers wrapper.SessionsAndSpeakers importedAdmins
    importHandles importedAdmins
    importHandles (importedSpeakersWithSessions |> Array.map (fun x -> x.Speaker))

    let preparedSessions = 
        importedSpeakersWithSessions
        |> Array.map (fun ss -> 
            let adminId = None
            match adminId with
            | Some _ -> {ss with Session = {ss.Session with AdminId = adminId; Status = "assigned"}}
            | None -> {ss with Session = {ss.Session with AdminId = adminId}} )

    let importedSession = 
        preparedSessions
        |> Array.map (fun s -> Session.postAndGetId s.Session)
    
    ()
