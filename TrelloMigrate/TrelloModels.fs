module TrelloModels
open System

type BasicCard = 
    { Id : string
      Name : string
      Due : DateTime option
      IdMembers : string [] }

type BoardSummary = 
    { BasicCards : BasicCard [] }
