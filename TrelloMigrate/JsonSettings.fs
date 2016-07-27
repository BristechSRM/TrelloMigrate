module JsonSettings

open JsonConverters
open Newtonsoft.Json

let setDefaults () = 
    JsonConvert.DefaultSettings <- (fun () -> 
        let settings = JsonSerializerSettings(
                        Formatting = Formatting.Indented,
                        ContractResolver = Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                       )
        settings.Converters.Add(OptionConverter())
        settings)