module Data.Bookings


open System
open Data.Common
open Domain
open Npgsql.FSharp

let getAllBookings (DbConnectionString connStr) = 
    connStr
    |> Sql.connect
    |> Sql.query "SELECT * FROM booking.Bookings"
    |> Sql.executeAsync (fun read ->
        {
            Id = read.uuid "id" |> BookingId
            Date = read.dateTime("bookingdatetime") |> DateOnly.FromDateTime
            Time = read.dateTime("bookingdatetime") |> TimeOnly.FromDateTime
            Seats = read.int "seats"
            SpecialRequest = read.stringOrNone "specialrequest"
            LocationId = read.uuid "locationid" |> LocationId
            ReservationId = read.uuid "reservationid" |> ReservationId
            CreatedOn = read.dateTime "createdon"
        })

let getBookingById (DbConnectionString connStr) (BookingId id) = task {
    let! reservations = 
        connStr
        |> Sql.connect
        |> Sql.query "SELECT * FROM booking.Bookings WHERE Id = @Id"
        |> Sql.parameters [ "Id", Sql.uuid id ]
        |> Sql.executeAsync (fun read ->
            {
                Id = read.uuid "Id" |> BookingId
                Date = read.dateTime("BookingDateTime") |> DateOnly.FromDateTime
                Time = read.dateTime("BookingDateTime") |> TimeOnly.FromDateTime
                Seats = read.int "Seats"
                SpecialRequest = read.stringOrNone "SpecialRequest"
                LocationId = read.uuid "LocationId" |> LocationId
                ReservationId = read.uuid "ReservationId" |> ReservationId
                CreatedOn = read.dateTime "CreatedOn"
            })
    
    return reservations |> List.tryHead
}

let createBooking (DbConnectionString connStr) (booking: BookingRequest) =
    connStr
    |> Sql.connect
    |> Sql.query "INSERT INTO booking.Bookings(Id, BookingDateTime, Seats, SpecialRequest, LocationId, ReservationId) VALUES (@Id, @RequestedBookingDateTime, @Seats, @SpecialRequest, @LocationId, @ReservationId)"
    |> Sql.parameters
           [
               "Id", Sql.uuid booking.Id.Value
               "BookingDateTime", Sql.timestamp (booking.Date.ToDateTime(booking.Time))
               "Seats", Sql.int booking.Seats
               "SpecialRequest", Sql.stringOrNone booking.SpecialRequest
               "LocationId", Sql.uuid booking.LocationId.Value
               "ReservationId", Sql.uuid booking.ReservationId.Value
           ]
    |> Sql.executeNonQueryAsync

