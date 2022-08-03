module Locations

open System
open Domain
open Giraffe
open Config
open Data.Common

let getAllLocations : HttpHandler =
    fun next ctx -> task {
        let dbConnStr = DbConnectionString ctx.Config.DbConnectionString
        let! locations = Data.Locations.getAllLocations dbConnStr
        return! json (locations |> List.map (fun l -> l.Serialise ()) |> List.toArray) next ctx
}

let getLocation (id: string) : HttpHandler =
    fun next ctx -> task {
        let dbConnStr = DbConnectionString ctx.Config.DbConnectionString
        let locationId = id |> Guid |> LocationId
        let! location = Data.Locations.getLocation dbConnStr locationId
        return! json (location.Serialise ()) next ctx
}


