module SrmImport

open SrmApiModels
open SrmApiClient

//Since everything in here is causing side effects, this module isn't as functional.
//Maybe this should be written in a imperative way instead to make that obvious? 

let private importAdmins (wrapper : SrmWrapper) =
    {wrapper with Admins = Array.map Profile.postAndGetId wrapper.Admins }
    
let importAll wrapper = 
    wrapper
    |> importAdmins
    |> ignore

