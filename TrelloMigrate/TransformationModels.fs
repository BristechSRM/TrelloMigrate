module TransformationModels

type Names = 
    { Forename : string
      Surname : string }

type ParsedCard = 
    { SpeakerName : string 
      SpeakerEmail : string option }