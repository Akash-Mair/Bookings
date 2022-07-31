module Reservations

open System
open Data.Common
open Domain
open Giraffe
open Config

module Dto =
    type ReservationRequest =
        {|
            Id: Guid
            Date: string
            Time: string 
            Seats: int
            SpecialRequest: string
            LocationId: Guid 
        |}
    
    let reservationRequestDtoToDomain (dto: ReservationRequest) =
        {
            Id = ReservationId dto.Id
            Date = DateOnly.Parse dto.Date
            Time = TimeOnly.Parse dto.Time
            Status = Requested
            Seats = dto.Seats
            SpecialRequest =
                if String.IsNullOrEmpty dto.SpecialRequest then
                    None
                else
                    Some dto.SpecialRequest
            LocationId = LocationId dto.LocationId
        }

let requestReservation : HttpHandler =
    fun next ctx -> task {
        let dbConnStr = DbConnectionString ctx.Config.DbConnectionString
        let! reservationRequestDto = ctx.BindJsonAsync<Dto.ReservationRequest>()
        let reservationRequest = Dto.reservationRequestDtoToDomain reservationRequestDto
        let! _ = Data.Reservations.createReservation dbConnStr reservationRequest 
        return! Successful.CREATED reservationRequest.Id.Value next ctx 
    }
    
let getAllReservations : HttpHandler =
    fun next ctx -> task {
        let dbConnStr = DbConnectionString ctx.Config.DbConnectionString
        let! reservations = Data.Reservations.getAllReservations dbConnStr
        let serialisedReservations =
            reservations
            |> List.map (fun r -> r.Serialise())
            |> List.toArray
        return! json serialisedReservations next ctx 
    }

let getReservationById (id: string) : HttpHandler =
    fun next ctx -> task {
        let dbConnStr = DbConnectionString ctx.Config.DbConnectionString
        let reservationId = (ReservationId (Guid id)) 
        let! res = Data.Reservations.getReservationById dbConnStr reservationId 
        return! json (res.Serialise()) next ctx
    }
