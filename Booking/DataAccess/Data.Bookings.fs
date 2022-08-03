module Data.Bookings


open System
open System.Data.SqlClient
open Data.Common
open Domain
open Npgsql.FSharp

let readBooking (read: RowReader) =
    {
        Id = read.uuid "id" |> BookingId
        Date = read.dateTime "bookingdatetime" |> DateOnly.FromDateTime
        Time = read.dateTime "bookingdatetime" |> TimeOnly.FromDateTime
        Seats = read.int "seats"
        SpecialRequest = read.stringOrNone "specialrequest"
        LocationId = read.uuid "locationid" |> LocationId
        ReservationId = read.uuid "reservationid" |> ReservationId
        CreatedOn = read.dateTime "createdon"
    }

let getAllBookings (DbConnectionString connStr) = 
    connStr
    |> Sql.connect
    |> Sql.query "SELECT * FROM booking.Bookings"
    |> Sql.executeAsync readBooking

let getBookingById (DbConnectionString connStr) (BookingId id) = 
    connStr
    |> Sql.connect
    |> Sql.query "SELECT * FROM booking.Bookings WHERE Id = @Id"
    |> Sql.parameters [ "Id", Sql.uuid id ]
    |> Sql.executeRowAsync readBooking

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

let getAllBookingsByDateBeforeTime (DbConnectionString connStr) (reservationDateTime: ReservationDateTimeRequest) =
    let endDateTime = reservationDateTime.Date.ToDateTime(reservationDateTime.Time)
    let startDateTime = reservationDateTime.Date.ToDateTime(TimeOnly.MinValue)
    
    connStr
    |> Sql.connect
    |> Sql.query "SELECT * FROM booking.Booking WHERE BookingDateTime BETWEEN @StartTime AND @EndTime"
    |> Sql.parameters [ "StartTime", Sql.timestamp startDateTime; "EndTime", Sql.timestamp endDateTime ]
    |> Sql.executeAsync readBooking

