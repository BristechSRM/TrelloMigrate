module TrelloClient

open Credentials
open System
open TrelloModels

//Note: If number of Cards get higher than 1000, then paging needs to be implemented. 
let getBasicCards trelloCred = 
    sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/cards/open?fields=id,name,idMembers,due&key=%s&token=%s" trelloCred.Key trelloCred.Token
    |> Uri
    |> JsonHttpClient.get<BasicCard []>

let getBasicMembers trelloCred = 
    sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/members?fields=id,username,fullName,avatarHash&key=%s&token=%s" trelloCred.Key trelloCred.Token
    |> Uri
    |> JsonHttpClient.get<BasicMember []>

let getBoardSummary trelloCred = 
    { BasicCards = getBasicCards trelloCred
      BasicMembers = getBasicMembers trelloCred }