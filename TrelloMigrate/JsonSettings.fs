module JsonSettings

open Newtonsoft.Json
open JsonConverters

let setDefaults () = 
    JsonConvert.DefaultSettings <- (fun () -> 
        let settings = JsonSerializerSettings(
                        Formatting = Formatting.Indented,
                        ContractResolver = Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                       )
        settings.Converters.Add(OptionConverter())
        settings)