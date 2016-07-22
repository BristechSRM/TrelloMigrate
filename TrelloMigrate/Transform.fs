module Transform

open System.Text.RegularExpressions
open TrelloModels
open SrmApiModels
open TransformationModels
open System

module private Profile = 

    //Note : Currently failing if any members (Admin) have a missing name
    //Take first string of split as first name, take the rest as last name
    let private parseFullName (fullName : string) = 
        let split = fullName.Split()
        match split.Length with
        | 1 -> { Forename = split.[0]; Surname = "" }
        | x when x > 1 -> { Forename = split.[0]; Surname = split |> Array.skip 1 |> String.concat " " }
        | _ -> 
            let message = "Full name of is missing? All Admins must have a full name, and all speaker cards must have the speaker name filled in. Input value was: " + fullName
            failwith message   

    let private defaultImageUrl = "https://placebear.com/50/50"

    let private getImageUrl avatarHash =   
        if String.IsNullOrWhiteSpace avatarHash then
            defaultImageUrl
        else 
            sprintf "https://trello-avatars.s3.amazonaws.com/%s/50.png" avatarHash

    let private create imageUrl (names : Names) : Profile = 
        { Id = Guid.Empty 
          Forename = names.Forename
          Surname = names.Surname
          Rating = 1 
          ImageUrl = imageUrl
          Bio = String.Empty }

    let fromMember (basicMember : BasicMember) = 
        parseFullName basicMember.FullName
        |> create (getImageUrl basicMember.AvatarHash)

    let fromNameString (fullName : string) = 
        parseFullName fullName
        |> create defaultImageUrl

module private Handle = 
    let createEmailHandle (email : string) = 
        { ProfileId = Guid.NewGuid(); Type = "email"; Identifier = email.ToLowerInvariant() }

module private Admin = 
    let private nameToScottLogicEmail (forename : string) (surname : string) = 
        if String.IsNullOrWhiteSpace surname then
            forename + "@scottlogic.co.uk"
        else 
            let firstNameFirstLetter = forename.Chars(0)
            let lastName = surname.Split() |> Array.last
            sprintf "%c%s@scottlogic.co.uk" firstNameFirstLetter lastName

    let create (basicMember : BasicMember) = 
        let profile = Profile.fromMember basicMember
        let handle = nameToScottLogicEmail profile.Forename profile.Surname |> Handle.createEmailHandle
        { ProfileWithHandles = 
              { Profile = profile
                Handles = [| handle |] }
          TrelloMemberId = basicMember.Id }

module private Speaker = 
    let createProfileWithHandles (parsedCard : ParsedCard) = 
        let speakerProfile = Profile.fromNameString parsedCard.SpeakerName
        let handles = 
            match parsedCard.SpeakerEmail with
            | Some email -> [| Handle.createEmailHandle email |]
            | None -> [||]

        { Profile = speakerProfile; Handles = handles }

module private Session = 
    let create (parsedCard : ParsedCard) = 
        { Id = Guid.Empty 
          Title = parsedCard.TalkData 
          Description = String.Empty
          Status = "unassigned" 
          Date = None
          SpeakerId = Guid.Empty 
          AdminId = None
          DateAdded = None }

module private SessionAndSpeaker = 

    let private tryParseCardName (cardName : string) = 
        let tryGetValue (group : Group) = 
            if group.Success && not <| String.IsNullOrWhiteSpace group.Value then Some <| group.Value.Trim()
            else None
        
        let m = Regex.Match(cardName, "(?<name>[^\[\]]* *)\[(?<email>.*)\] *\((?<talk>.*)\)(?<extra>.*)?$", RegexOptions.ExplicitCapture)
        if m.Success && m.Groups.["name"].Success && not <| String.IsNullOrWhiteSpace m.Groups.["name"].Value then 
            { SpeakerName = m.Groups.["name"].Value.Trim() 
              SpeakerEmail = tryGetValue m.Groups.["email"] 
              TalkData = m.Groups.["talk"].Value.Trim() }
            |> Some
        else None

    //Note: Currently ignoring any cards that don't have the title (name) filled out correctly. 
    let tryCreate (admins : Admin []) (card : BasicCard) = 
        match tryParseCardName card.Name with
        | Some parsedCard -> 
            let adminId = 
                match card.IdMembers with
                | [||]  -> None
                | [| memberId |] -> 
                    //If the member isn't in the admin list, it will be ignored. 
                    admins |> Array.tryPick(fun admin -> if admin.TrelloMemberId = memberId then Some admin.TrelloMemberId else None)
                | _ -> 
                    failwith <| sprintf "Card: %A had multiple members attached. Please remove additional members so that there is one admin per card" card
            Some { Session = Session.create parsedCard
                   Speaker = Speaker.createProfileWithHandles parsedCard 
                   AdminTrelloId = adminId }
        | None -> 
            printfn "Card with title:\n'%s' \nwas ingored because it did not match the accepted format of \n'speaker name[speakeremail](Talk title, brief or possible topic)" card.Name
            None        

//TODO deal with ignored admins when creating sessions
let toSrmModels (board : BoardSummary) = 
    let admins = board.Members |> Array.map Admin.create
    let sessionsAndSpeakers = board.BasicCards |> Array.choose (SessionAndSpeaker.tryCreate admins)
    { Admins = admins
      SessionsAndSpeakers = sessionsAndSpeakers}   