module TransformationModels
open SrmApiModels

type Names = 
    { Forename : string
      Surname : string }

type ParsedCardName = 
    { SpeakerName : string 
      SpeakerEmail : string option 
      TalkData : string }

type ParsedCard = 
    { Session : Session 
      Speaker : ProfileWithHandles 
      CardTrelloId : string
      AdminTrelloId : string option }
