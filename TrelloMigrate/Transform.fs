module Transform

open System.Text.RegularExpressions
open TrelloModels
open SrmApiModels
open TransformationModels
open System

module private Profile = 

    //Note : Currently failing if any members (Admin) have a missing name
    //Take first string of split as first name, take the rest as last name
    let parseFullName (fullName : string) = 
        let split = fullName.Split()
        match split.Length with
        | 1 -> { Forename = split.[0]; Surname = "" }
        | x when x > 1 -> { Forename = split.[0]; Surname = split |> Array.skip 1 |> String.concat " " }
        | _ -> 
            let message = "Full name of member is somehow missing? Make sure everyone on the trello board enters a full name. Input value was: " + fullName
            failwith message   

    let defaultImage = "https://placebear.com/50/50"
    let getImageUrl avatarHash =   
        if String.IsNullOrWhiteSpace avatarHash then
            defaultImage
        else 
            sprintf "https://trello-avatars.s3.amazonaws.com/%s/50.png" avatarHash

    let create imageUrl (names : Names) : Profile = 
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
        |> create defaultImage

module private Speaker = 

    let expectedNumberOfGroupsInCardParse = 5
    let speakerNameGroup = 1

    let allGroupsMatched (groups : GroupCollection) = 
        groups.Count = expectedNumberOfGroupsInCardParse && not <| String.IsNullOrWhiteSpace(groups.[speakerNameGroup].Value)

    let tryGetNameRegexGroups (card : BasicCard) = 
        let m = Regex.Match(card.Name, "(.*)\[(.*)\]\((.*)\)(.*)$")
        if m.Success && allGroupsMatched m.Groups then Some m.Groups 
        else None

    //Note: Currently ignoring any cards that don't have the title (name) filled out correctly. 
    let parseOrIgnoreCard (card : BasicCard) = 
        match tryGetNameRegexGroups card with
        | Some groups -> 
            let speakerProfile = Profile.fromNameString groups.[speakerNameGroup].Value
            Some speakerProfile
        | None -> 
            printfn "Card with title:\n'%s' \nwas ingored because it did not match the accepted format of \n'speaker name[speakeremail](Talk title, brief or possible topic)" card.Name
            None

let toSrmSummary (board : BoardSummary) = 
    { Admins = board.GroupedMembers.Members |> Array.map Profile.fromMember 
      Speakers = board.BasicCards |> Array.choose Speaker.parseOrIgnoreCard }   