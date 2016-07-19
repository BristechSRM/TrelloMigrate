module Transform

open TrelloModels
open SrmApiModels
open System

let memberToProfile (bMember : BasicMember) : Profile = 
    { Id = Guid.Empty 
      Forename = bMember.FullName
      Surname = bMember.FullName
      Rating = 1 
      ImageUrl = String.Empty
      Bio = String.Empty }

let toSrmOutline (board : BoardSummary) = 
    { Profiles = board.BasicMembers |> Array.map memberToProfile }   