module Data.Reservations

open System
open Data.Common
open Domain
open Npgsql.FSharp

let getAllReservations (DbConnectionString connStr) = 
    let sqlQuery =
        """SELECT * FROM Reservations r JOIN Locations l ON r.LocationId = l.Id"""
    
    connStr
    |> Sql.connect
    |> Sql.query sqlQuery
    |> Sql.executeAsync (fun read ->
        {
            Id = read.uuid "Id" |> ReservationId
            Date = read.dateTime("Date") |> DateOnly.FromDateTime
            Time = read.dateTime("Time") |> TimeOnly.FromDateTime
            Seats = read.int "Seats"
            SpecialRequest = read.stringOrNone "SpecialRequest"
            Location = read.uuid "LocationId" |> LocationId
            Status = read.string "Status" |> ReservationStatus.Deserialise
        })


let updateReservationStatus (DbConnectionString connStr) (ReservationId id) (status: ReservationStatus) =
    let sqlQuery =
        "UPDATE Reservations SET Status = @Status WHERE Id = @ReservationId"
    
    connStr
    |> Sql.connect
    |> Sql.query sqlQuery
    |> Sql.parameters [ "Status", Sql.string (status.Serialise()); "ReservationId", Sql.uuid id ]
    |> Sql.executeNonQueryAsync
