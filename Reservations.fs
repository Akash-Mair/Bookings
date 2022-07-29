module Reservations

open System
open Data.Common
open Domain
open Giraffe
open Microsoft.AspNetCore.Http
open Config
open Microsoft.Extensions.Configuration 

module Dto =
    type ReservationRequestDto =
        {|
            Id: Guid
            Date: DateOnly
            Time: TimeOnly
            Seats: int
            SpecialRequest: string option
            LocationId: Guid
            Status: string 
        |}
    
    let reservationRequestDtoToDomain (dto: ReservationRequestDto) =
        {
            Id = ReservationId dto.Id
            Date = dto.Date
            Time = dto.Time
            Status = dto.Status |> ReservationStatus.Deserialise
            Seats = dto.Seats
            SpecialRequest = dto.SpecialRequest
            LocationId = LocationId dto.LocationId
        }

type HttpContext with
    member ctx.Config =
        ctx.GetService<IConfiguration>()

let requestReservation : HttpHandler =
    fun next ctx -> task {
        let dbConnStr = DbConnectionString ctx.Config.DbConnectionString
        let! reservationRequestDto = ctx.BindJsonAsync<Dto.ReservationRequestDto>()
        let reservationRequest = Dto.reservationRequestDtoToDomain reservationRequestDto
        let! _ = Data.Reservations.createReservation dbConnStr reservationRequest 
        return! Successful.CREATED reservationRequestDto.Id next ctx 
    }
