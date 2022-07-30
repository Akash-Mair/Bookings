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
            Date: DateOnly
            Time: TimeOnly
            Seats: int
            SpecialRequest: string option
            LocationId: Guid
            Status: string 
        |}
    
    let reservationRequestDtoToDomain (dto: ReservationRequest) =
        {
            Id = ReservationId dto.Id
            Date = dto.Date
            Time = dto.Time
            Status = dto.Status |> ReservationStatus.Deserialise
            Seats = dto.Seats
            SpecialRequest = dto.SpecialRequest
            LocationId = LocationId dto.LocationId
        }

let requestReservation : HttpHandler =
    fun next ctx -> task {
        let dbConnStr = DbConnectionString ctx.Config.DbConnectionString
        let! reservationRequestDto = ctx.BindJsonAsync<Dto.ReservationRequest>()
        let reservationRequest = Dto.reservationRequestDtoToDomain reservationRequestDto
        let! _ = Data.Reservations.createReservation dbConnStr reservationRequest 
        return! Successful.CREATED reservationRequestDto.Id next ctx 
    }
    
let getAllReservations : HttpHandler =
    fun next ctx -> task {
        let dbConnStr = DbConnectionString ctx.Config.DbConnectionString
        let! reservations = Data.Reservations.getAllReservations dbConnStr  
        return! json (List.toArray reservations) next ctx 
    }

let getReservationById (id: string) : HttpHandler =
    fun next ctx -> task {
        let dbConnStr = DbConnectionString ctx.Config.DbConnectionString
        let reservationId = (ReservationId (Guid id)) 
        match! Data.Reservations.getReservationById dbConnStr reservationId with
        | Some reservation ->
            return! json reservation next ctx
        | None ->
            return! text "That reservation does not exist" next ctx 
    }
