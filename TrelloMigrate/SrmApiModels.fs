module SrmApiModels
open System

type Profile = 
    { Id : Guid
      Forename : string
      Surname : string
      Rating : int
      ImageUrl : string
      Bio : string }

type Handle = 
    { ProfileId : Guid 
      Type : string
      Identifier : string }

type ProfileWithHandles = 
    { Profile : Profile
      Handle : Handle [] }

type SrmWrapper = 
    { Admins : ProfileWithHandles [] 
      Speakers : ProfileWithHandles [] }