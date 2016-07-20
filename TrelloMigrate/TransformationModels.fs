module TransformationModels
open SrmApiModels

type Names = 
    { Forename : string
      Surname : string }

type CardNameParseData = 
    { SpeakerName : string }