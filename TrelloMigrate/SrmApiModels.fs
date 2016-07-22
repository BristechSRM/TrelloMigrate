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
      Handles : Handle [] }

type Session = 
     { Id : Guid 
       Title : string
       Description : string
       Status : string
       Date : DateTime option
       SpeakerId : Guid
       AdminId : Guid option 
       DateAdded : DateTime option }

type SessionAndSpeaker = 
    { Session : Session 
      Speaker : ProfileWithHandles }

type SrmWrapper = 
    { Admins : ProfileWithHandles [] 
      SessionsAndSpeakers : SessionAndSpeaker [] }