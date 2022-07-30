module Bookings

open System
open System.Transactions
open Data.Common
open Domain
open Giraffe
open Config

module Dto =
    type BookingRequest =
        {|
            Id: Guid
            Date: DateOnly
            Time: TimeOnly
            Seats: int
            SpecialRequest: string option
            LocationId: Guid
            ReservationId: Guid 
        |}
    
    let bookingRequestDtoToDomain (dto: BookingRequest) =
        {
            Id = BookingId dto.Id
            Date = dto.Date
            Time = dto.Time
            Seats = dto.Seats
            SpecialRequest = dto.SpecialRequest
            LocationId = LocationId dto.LocationId
            ReservationId = ReservationId dto.ReservationId
        }



let requestBooking : HttpHandler =
    fun next ctx -> task {
        use transaction = new TransactionScope()
        let dbConnStr = DbConnectionString ctx.Config.DbConnectionString
        let! bookingRequestDto = ctx.BindJsonAsync<Dto.BookingRequest>()
        let bookingRequest = Dto.bookingRequestDtoToDomain bookingRequestDto
        let! _ = Data.Bookings.createBooking dbConnStr bookingRequest
        let! _ = Data.Reservations.updateReservationStatus dbConnStr bookingRequest.ReservationId Successful
        transaction.Complete()
        return! Successful.CREATED bookingRequestDto.Id next ctx 
    }
    
let getAllBookings : HttpHandler =
    fun next ctx -> task {
        let dbConnStr = DbConnectionString ctx.Config.DbConnectionString
        let! bookings = Data.Bookings.getAllBookings dbConnStr  
        return! json (List.toArray bookings) next ctx 
    }

let getBookingById (id: string) : HttpHandler =
    fun next ctx -> task {
        let dbConnStr = DbConnectionString ctx.Config.DbConnectionString
        let bookingId = BookingId (Guid id)
        match! Data.Bookings.getBookingById dbConnStr bookingId with
        | Some booking ->
            return! json booking next ctx
        | None ->
            return! text "That booking does not exist" next ctx 
    }


