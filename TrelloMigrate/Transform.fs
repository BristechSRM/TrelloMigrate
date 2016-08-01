module Transform

open SrmApiModels
open System
open System.Globalization
open System.Text.RegularExpressions
open TransformationModels
open TrelloModels

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

    let private create imageUrl isAdmin (names : Names) : Profile = 
        { Id = Guid.Empty 
          Forename = names.Forename
          Surname = names.Surname
          Rating = 1 
          ImageUrl = imageUrl
          Bio = String.Empty 
          IsAdmin = isAdmin }

    let createAdmin (basicMember : BasicMember) = parseFullName basicMember.FullName |> create (getImageUrl basicMember.AvatarHash) true

    let createSpeaker (fullName : string) = parseFullName fullName |> create defaultImageUrl false

module private Handle = 
    let createEmailHandle (email : string) = 
        { ProfileId = Guid.NewGuid()
          Type = "email"
          Identifier = email.ToLowerInvariant() }

module private Admin = 
    //TODO use external list of emails / handles for admins, as not all admins are from scott logic. 
    let private nameToScottLogicEmail (forename : string) (surname : string) = 
        if String.IsNullOrWhiteSpace surname then
            forename + "@scottlogic.co.uk"
        else 
            let firstNameFirstLetter = forename.Chars(0)
            let lastName = surname.Split() |> Array.last
            sprintf "%c%s@scottlogic.co.uk" firstNameFirstLetter lastName

    let create (basicMember : BasicMember) : ProfileWithReferenceId = 
        let profile = Profile.createAdmin basicMember
        let handle = nameToScottLogicEmail profile.Forename profile.Surname |> Handle.createEmailHandle
        { ReferenceId = basicMember.Id
          ProfileWithHandles = 
              { Profile = profile
                Handles = [| handle |] } }

module private Speaker = 
    let createProfileWithHandles (parsedCardName : ParsedCardName) = 
        let speakerProfile = Profile.createSpeaker parsedCardName.SpeakerName
        
        let handles = 
            match parsedCardName.SpeakerEmail with
            | Some email -> [| Handle.createEmailHandle email |]
            | None -> [||]
        { Profile = speakerProfile
          Handles = handles }

module private Session = 
    //Accept date if it is on the first Thursday of the month. 
    let useDateIfEventDate (date : DateTime) = 
        if date.DayOfWeek = DayOfWeek.Thursday && date.Day <= 7 then
            Some date
        else 
            None

    let create (parsedCardName : ParsedCardName) (cardDate : DateTime option) = 
        { Id = Guid.Empty 
          Title = parsedCardName.TalkData 
          Description = String.Empty
          Status = String.Empty
          Date = cardDate |> Option.bind useDateIfEventDate
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

    let private tryPickAdminId (admins : ProfileWithReferenceId []) (card : BasicCard) = 
        match card.IdMembers with
        | [||]  -> None
        | [| memberId |] -> 
            //If the member isn't in the admin list, it will be ignored. 
            admins |> Array.tryPick(fun a -> if a.ReferenceId = memberId then Some memberId else None)
        | _ -> 
            failwith <| sprintf "Card: %A had multiple members attached. Please remove additional members so that there is one admin per card" card

    let tryParseFromCard (admins : ProfileWithReferenceId []) (card : BasicCard) = 
        match tryParseCardName card.Name with
        | Some parsedCard -> 
            Some { Session = Session.create parsedCard card.Due
                   Speaker = Speaker.createProfileWithHandles parsedCard
                   CardTrelloId = card.Id
                   AdminTrelloId = tryPickAdminId admins card }
        | None -> 
            printfn "Card with title:\n'%s' \nwas ingored because it did not match the accepted format of \n'speaker name[speakeremail](Talk title, brief or possible topic)" card.Name
            None        

    let splitProfilesFromSessions (admins : ProfileWithReferenceId []) (parsedCards : ParsedCard []) =
        //TODO perform a merge of speaker and admin information to make sure no information is lost. Currently extra handles would be dropped. 
        let sessions, nonAdminSpeakerOptions = 
            parsedCards
            |> Array.map (fun ss -> 
                let foundSpeakerAsAdmin = 
                    admins |> Array.tryFind(fun a -> a.ProfileWithHandles.Profile.Forename = ss.Speaker.Profile.Forename && a.ProfileWithHandles.Profile.Surname = ss.Speaker.Profile.Surname)
                match foundSpeakerAsAdmin with 
                | Some adminWithRef -> 
                    {Session = ss.Session; SpeakerTrelloId = adminWithRef.ReferenceId; AdminTrelloId = ss.AdminTrelloId}, None
                | None -> 
                    {Session = ss.Session; SpeakerTrelloId = ss.CardTrelloId; AdminTrelloId = ss.AdminTrelloId}, Some {ReferenceId = ss.CardTrelloId; ProfileWithHandles = ss.Speaker})
            |> Array.unzip
        let nonAdminSpeakers = nonAdminSpeakerOptions |> Array.choose id
        let profiles = admins |> Array.append nonAdminSpeakers |> Array.map(fun pri -> pri.ReferenceId, pri.ProfileWithHandles) |> Map.ofArray
        profiles, sessions

module private Correspondence = 
    let private tryGetEmailFromProfile (profile : ProfileWithHandles) = 
        profile.Handles |> Array.tryPick (fun h -> if h.Type = "email" then Some h.Identifier else None)

    let private parseDate (actionText : string) (dateString : string) = 
        //Workaround for data error + mono bug
        let success, exactDate = 
            DateTime.TryParseExact(dateString, [| "dd/mm/yy"; "dd/mm/yyyy"; "dd.mm.yy"; "dd.mm.yyyy"; "dd/mm.yyyy"; "dd.mm/yyyy" |], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
        if success then 
            exactDate
        else 
            try
                DateTime.Parse(dateString, CultureInfo.InvariantCulture)
            with
            | ex -> failwith <| sprintf "Parsing date for correspondence action: '%s' failed with exception: %A" actionText ex

    let private selectSenderAndReceiver (actionText : string) (direction : string) speakerValue adminValue = 
        match direction.ToUpperInvariant() with
        | "SEND" -> adminValue, speakerValue
        | "RECEIVE" | "RECIEVE" -> speakerValue, adminValue
        | _ -> failwith <| sprintf "Parsing directon for correspondence action: '%s' failed" actionText

    let private tryCreateCorrespondence (cardTitle : string) (speakerRefId : string) (speakerEmail : string) (adminRefIdOption : string option) (adminEmailOption : string option) (action : BasicAction) = 
        let m = Regex.Match(action.Data.Text, "\[(?<date>[0-9\/\.]+) - (?<direction>[a-zA-Z]+)\](?<message>.*)$", RegexOptions.ExplicitCapture ||| RegexOptions.Singleline)
        if m.Success then 
            match adminEmailOption, adminRefIdOption with
            | Some adminEmail, Some adminRefId  ->                 
                let senderEmail, receiverEmail = selectSenderAndReceiver action.Data.Text (m.Groups.["direction"].Value) speakerEmail adminEmail
                let senderTrelloId, receiverTrelloId = selectSenderAndReceiver action.Data.Text (m.Groups.["direction"].Value) speakerRefId adminRefId
                let item =                     
                    { Id = Guid.Empty 
                      ExternalId = sprintf "Trello:%A:%s:%s" (Guid.NewGuid()) senderEmail receiverEmail
                      SenderId = Guid.Empty
                      ReceiverId = Guid.Empty
                      Type = "email"
                      SenderHandle = senderEmail
                      ReceiverHandle = receiverEmail
                      Date = parseDate action.Data.Text m.Groups.["date"].Value
                      Message = m.Groups.["message"].Value }

                Some { Item = item; SenderTrelloId = senderTrelloId; ReceiverTrelloId = receiverTrelloId}
            | _ -> failwith <| sprintf "Card: '%s' has correspondence comments, but no admin or adminEmail. An Admin is required for correspondence to be processed." cardTitle
        else 
            None

    let parseActionsPerSession (profiles : Map<string, ProfileWithHandles>) (cardActions : Map<string, BasicAction []>) (sr : SessionAndReferences) = 
        match cardActions.TryFind(sr.SpeakerTrelloId) with
        | Some cardActions -> 
            let speakerEmailOption = profiles.[sr.SpeakerTrelloId] |> tryGetEmailFromProfile
            match speakerEmailOption with
            | Some speakerEmail -> 
                let adminEmailOption = sr.AdminTrelloId |> Option.bind (fun pid -> tryGetEmailFromProfile profiles.[pid])
                cardActions |> Array.choose (tryCreateCorrespondence sr.Session.Title sr.SpeakerTrelloId speakerEmail sr.AdminTrelloId adminEmailOption)
            | None -> 
                printfn "Card: '%s' has possible correspondence comments, but no speakerEmail. Correspondence will not be processed." sr.Session.Title
                [||]
        | None -> [||] 

    let parseActions (sessions : SessionAndReferences []) (profiles : Map<string, ProfileWithHandles>) (cardActions : Map<string, BasicAction []>) = 
        sessions
        |> Array.map (parseActionsPerSession profiles cardActions)
        |> Array.concat

let toSrmModels (board : BoardSummary) = 
    printf "Transforming all data for migration"
    let admins = board.Members |> Array.map Admin.create
    let parsedCards = board.BasicCards |> Array.choose (SessionAndSpeaker.tryParseFromCard admins)
    let profiles, sessions = SessionAndSpeaker.splitProfilesFromSessions admins parsedCards
    let correspondence = Correspondence.parseActions sessions profiles board.CardActions
    { Correspondence = correspondence
      Profiles = profiles
      Sessions = sessions }   
