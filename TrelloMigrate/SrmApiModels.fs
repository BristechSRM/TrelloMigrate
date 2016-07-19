module SrmApiModels
open System

type Profile = 
    { Id : Guid
      Forename : string
      Surname : string
      Rating : int
      ImageUrl : string
      Bio : string }

type SpeakerAndSession = 
    { Speaker : Profile 
      Session : string } //TODO

type SrmOutline = 
    { Admins : Profile [] 
      SpeakersAndSessions : SpeakerAndSession [] }