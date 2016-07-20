module TransformationModels
open SrmApiModels

type Names = 
    { Forename : string
      Surname : string }

type CardNameParseData = 
    { SpeakerName : string 
      SpeakerEmail : string option 
      TalkData : string option 
      ExtraData : string option }