module TrelloClient

open Credentials
open System
open TrelloModels

//Note: If number of Cards get higher than 1000, then paging needs to be implemented. 
let private getBasicCards trelloCred = 
    sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/cards/open?fields=id,name,idMembers,due&key=%s&token=%s" trelloCred.Key trelloCred.Token
    |> Uri
    |> JsonHttpClient.get<BasicCard []>

let private getBasicMembers trelloCred = 
    sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/members?fields=id,username,fullName,avatarHash&key=%s&token=%s" trelloCred.Key trelloCred.Token
    |> Uri
    |> JsonHttpClient.get<BasicMember []>

let private getBasicActionsByCardId trelloCred (cardId : string) = 
    sprintf "https://api.trello.com/1/cards/%s/actions?filter=commentCard&key=%s&token=%s" cardId trelloCred.Key trelloCred.Token
    |> Uri
    |> JsonHttpClient.get<BasicAction []>

let getBoardSummary trelloCred = 
    let basicCards = getBasicCards trelloCred
    
    let cardActions = 
        basicCards
        |> Array.map (fun card -> card.Id, getBasicActionsByCardId trelloCred card.Id)
        |> Array.filter (fun (_, actions) -> actions.Length > 0)
        |> Map.ofArray
    { BasicCards = basicCards
      CardActions = cardActions
      Members = getBasicMembers trelloCred }
