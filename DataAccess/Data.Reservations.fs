module Data.Reservations

open System
open System.ComponentModel
open System.Transactions
open Data.Common
open Domain
open Npgsql.FSharp
open Amazon.SQS

let readReservation (read: RowReader) = 
    {
        Id = read.uuid "id" |> ReservationId
        Date = read.dateTime "requestedbookingdatetime" |> DateOnly.FromDateTime
        Time = read.dateTime "requestedbookingdatetime" |> TimeOnly.FromDateTime
        Seats = read.int "seats"
        SpecialRequest = read.textOrNone "specialrequest"
        LocationId = read.uuid "locationid" |> LocationId
        Status = read.text "status" |> ReservationStatus.Deserialise
        CreatedOn = read.dateTime "createdon"
    }

let getAllReservations (DbConnectionString connStr) = 
    connStr
    |> Sql.connect
    |> Sql.query "SELECT * FROM booking.Reservations"
    |> Sql.executeAsync readReservation

let getReservationById (DbConnectionString connStr) (ReservationId id) =
    connStr
    |> Sql.connect
    |> Sql.query "SELECT * FROM booking.Reservations WHERE Id = @ResId"
    |> Sql.parameters [ "@ResId", Sql.uuid id ]
    |> Sql.executeRowAsync readReservation

let updateReservationStatus (DbConnectionString connStr) (ReservationId id) (status: ReservationStatus) =
    connStr
    |> Sql.connect
    |> Sql.query "UPDATE booking.Reservations SET Status = @Status WHERE Id = @ReservationId"
    |> Sql.parameters
           [
               "@Status", Sql.string (status.Serialise())
               "@ReservationId", Sql.uuid id
           ]
    |> Sql.executeNonQueryAsync

let createReservation (DbConnectionString connStr) (sqsClient: IAmazonSQS) (reservation: ReservationRequest) = task {
    use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
    let! affectedRows =
        connStr
        |> Sql.connect
        |> Sql.query
               "INSERT INTO booking.Reservations(Id, RequestedBookingDateTime, Seats, SpecialRequest, LocationId, Status) VALUES (@Id, @RequestedBookingDateTime, @Seats, @SpecialRequest, @LocationId, @Status)"
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
    if affectedRows > 0 then
        let queueName = "reservation-queue"
        let! reservationQueue = sqsClient.CreateQueueAsync queueName
        let! _ = sqsClient.SendMessageAsync(reservationQueue.QueueUrl, $"{reservation.Id.Value}")
        ()
    transaction.Complete()
}