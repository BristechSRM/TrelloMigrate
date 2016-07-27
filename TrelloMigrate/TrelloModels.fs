module TrelloModels
open System

type BasicCard = 
    { Id : string
      Name : string
      Due : DateTime option
      IdMembers : string [] }

type BasicMember = 
    { Id : string
      Username : string
      FullName : string
      AvatarHash : string }

type ActionData = 
    { Text : string }

type BasicAction = 
    { Id : string
      Data : ActionData
      Type : string }

type BoardSummary = 
    { BasicCards : BasicCard [] 
      CardActions : Map<string, BasicAction []>
      Members : BasicMember [] }
