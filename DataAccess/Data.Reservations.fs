module Data.Reservations

open System
open Data.Common
open Domain
open Npgsql.FSharp

let getAllReservations (DbConnectionString connStr) = 
    connStr
    |> Sql.connect
    |> Sql.query "SELECT * FROM Reservations"
    |> Sql.executeAsync (fun read ->
        {
            Id = read.uuid "Id" |> ReservationId
            Date = read.dateTime("RequestedBookingDateTime") |> DateOnly.FromDateTime
            Time = read.dateTime("RequestedBookingDateTime") |> TimeOnly.FromDateTime
            Seats = read.int "Seats"
            SpecialRequest = read.stringOrNone "SpecialRequest"
            LocationId = read.uuid "LocationId" |> LocationId
            Status = read.string "Status" |> ReservationStatus.Deserialise
            CreatedOn = read.dateTime "CreatedOn"
        })

let getReservationById (DbConnectionString connStr) (ReservationId id) = task {
    let! reservations = 
        connStr
        |> Sql.connect
        |> Sql.query "SELECT * FROM Reservations WHERE Id = @Id"
        |> Sql.parameters [ "Id", Sql.uuid id ]
        |> Sql.executeAsync (fun read ->
            {
                Id = read.uuid "Id" |> ReservationId
                Date = read.dateTime("RequestedBookingDateTime") |> DateOnly.FromDateTime
                Time = read.dateTime("RequestedBookingDateTime") |> TimeOnly.FromDateTime
                Seats = read.int "Seats"
                SpecialRequest = read.stringOrNone "SpecialRequest"
                LocationId = read.uuid "LocationId" |> LocationId
                Status = read.string "Status" |> ReservationStatus.Deserialise
                CreatedOn = read.dateTime "CreatedOn"
            })
    
    return reservations |> List.tryHead
}

let updateReservationStatus (DbConnectionString connStr) (ReservationId id) (status: ReservationStatus) =
    connStr
    |> Sql.connect
    |> Sql.query "UPDATE Reservations SET Status = @Status WHERE Id = @ReservationId"
    |> Sql.parameters [ "@Status", Sql.string (status.Serialise()); "@ReservationId", Sql.uuid id ]
    |> Sql.executeNonQueryAsync

let createReservation (DbConnectionString connStr) (reservation: ReservationRequest) =
    connStr
    |> Sql.connect
    |> Sql.query "INSERT INTO Reservations(Id, RequestedBookingDateTime, Seats, SpecialRequest, LocationId, Status) VALUES (@Id, @RequestedBookingDateTime, @Seats, @SpecialRequest, @LocationId, @Status)"
    |> Sql.parameters
           [
               "Id", Sql.uuid reservation.Id.Value
               "RequestedBookingDateTime", Sql.timestamp (reservation.Date.ToDateTime(reservation.Time))
               "Seats", Sql.int reservation.Seats
               "SpecialRequest", Sql.stringOrNone reservation.SpecialRequest
               "LocationId", Sql.uuid reservation.LocationId.Value
               "Status", Sql.string (reservation.Status.Serialise ())
           ]
    |> Sql.executeNonQueryAsync
