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

type ProfileWithReferenceId = 
    { ReferenceId : string
      ProfileWithHandles : ProfileWithHandles }

type Session = 
     { Id : Guid 
       Title : string
       Description : string
       Status : string
       Date : DateTime option
       SpeakerId : Guid
       AdminId : Guid option 
       DateAdded : DateTime option }

type SessionSpeakerAndTrelloIds = 
    { Session : Session 
      Speaker : ProfileWithHandles 
      CardTrelloId : string
      AdminTrelloId : string option }

type SessionAndTrelloReferences = 
    { Session : Session 
      SpeakerTrelloId : string 
      AdminTrelloId : string option }

type SrmWrapper = 
    { Profiles : Map<string,ProfileWithHandles>
      Sessions : SessionAndTrelloReferences [] }