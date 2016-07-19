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

let private groupMembers (members) = 
    let ignoredAdminUserNames = [ "samdavies"; "jamesphillpotts"; "tamarachehayebmakarem1"; "tamaramakarem"; "nicholashemley" ]
    members
    |> Array.partition (fun bMember -> ignoredAdminUserNames |> List.contains bMember.Username)
    |> (fun (ignoredMembers, keptMembers) -> { Members = keptMembers; IgnoredMembers = ignoredMembers } )

let getBoardSummary trelloCred =     
    { BasicCards = getBasicCards trelloCred
      GroupedMembers = getBasicMembers trelloCred |> groupMembers}