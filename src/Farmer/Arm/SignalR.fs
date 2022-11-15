[<AutoOpen>]
module Farmer.Arm.SignalRService

open Farmer
open Farmer.SignalR

let signalR = ResourceType("Microsoft.SignalRService/signalR", "2018-10-01")

type UpstreamConfig =
    {
        UrlTemplate: string
        HubPattern: string
        CategoryPattern: string
        EventPattern: string
    }

type SignalR =
    {
        Name: ResourceName
        Location: Location
        Sku: Sku
        Capacity: int option
        AllowedOrigins: string list
        ServiceMode: ServiceMode
        Tags: Map<string, string>
        UpstreamConfigs: UpstreamConfig[]
    }

    interface IArmResource with
        member this.ResourceId = signalR.resourceId this.Name

        member this.JsonModel =
            {| signalR.Create(this.Name, this.Location, tags = this.Tags) with
                sku =
                    {|
                        name =
                            match this.Sku with
                            | Free -> "Free_F1"
                            | Standard -> "Standard_S1"
                        capacity =
                            match this.Capacity with
                            | Some c -> c.ToString()
                            | None -> null
                    |}
                properties =
                    {|
                        cors =
                            match this.AllowedOrigins with
                            | [] -> null
                            | aos -> box {| allowedOrigins = aos |}
                        features =
                            [
                                {|
                                    flag = "ServiceMode"
                                    value = this.ServiceMode.ToString()
                                |}
                            ]
                    |}
                upstream = 
                    {| 
                        templates = 
                            this.UpstreamConfigs
                            |> List.map(fun config ->
                                {|
                                    categoryPattern = config.CategoryPattern
                                    eventPattern = config.EventPattern
                                    hubPattern = config.HubPattern
                                    urlTemplate = config.UrlTemplate
                                |})    
                    |}
            |}
