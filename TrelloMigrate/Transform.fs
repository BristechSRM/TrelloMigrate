module Transform

open TrelloModels
open SrmApiModels
open System

//Take first string of split as first name, take the rest as last name
let private parseFullName (fullName : string) = 
    let split = fullName.Split()
    match split.Length with
    | 1 -> split.[0], ""
    | x when x > 1 -> 
        let lastName = split |> Array.skip 1 |> String.concat " " 
        split.[0],lastName
    | _ -> 
        let message = "Full name of member is somehow missing? Make sure everyone on the trello board enters a full name. Input value was: " + fullName
        failwith message   

let private getImageUrl avatarHash =   
    if String.IsNullOrWhiteSpace avatarHash then
        "https://placebear.com/50/50"
    else 
        sprintf "https://trello-avatars.s3.amazonaws.com/%s/50.png" avatarHash

let private memberToProfile (bMember : BasicMember) : Profile = 
    let forename, surname = parseFullName bMember.FullName
    { Id = Guid.Empty 
      Forename = forename
      Surname = surname
      Rating = 1 
      ImageUrl = getImageUrl bMember.AvatarHash
      Bio = String.Empty }

let toSrmOutline (board : BoardSummary) = 
    { Profiles = board.BasicMembers |> Array.map memberToProfile }   