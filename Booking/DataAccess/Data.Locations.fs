module Data.Locations

open Data.Common
open Domain
open Npgsql.FSharp

let readLocation (read : RowReader) =
    {
        Id = read.uuid "Id" |> LocationId
        Name = read.text "Name" |> LocationName
        MaxCapacity = read.int "MaxCapacity" 
    }

let getAllLocations (DbConnectionString connStr) =
    connStr
    |> Sql.connect
    |> Sql.query "SELECT * FROM booking.Locations"
    |> Sql.executeAsync readLocation 

let getLocation (DbConnectionString connStr) (LocationId id) =
    connStr
    |> Sql.connect
    |> Sql.query "SELECT * FROM booking.Locations WHERE Id = @Id"
    |> Sql.parameters [ "Id", Sql.uuid id ]
    |> Sql.executeRowAsync readLocation

