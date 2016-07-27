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

type SessionAndReferences = 
    { Session : Session 
      SpeakerTrelloId : string 
      AdminTrelloId : string option }

type CorrespondenceItem =
    { Id : Guid
      ExternalId : string
      SenderId : Guid
      ReceiverId : Guid 
      Date : DateTime 
      Message : string
      Type : string
      SenderHandle : string
      ReceiverHandle : string }

type CorrespondenceWithReferences = 
    { Item : CorrespondenceItem 
      SenderTrelloId : string
      ReceiverTrelloId : string }

type SrmWrapper = 
    { Correspondence : CorrespondenceWithReferences []
      Profiles : Map<string,ProfileWithHandles>
      Sessions : SessionAndReferences [] }