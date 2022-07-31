module Reservations

open System
open System.Transactions
open Data.Common
open Domain
open Giraffe
open Config
open Amazon.SQS

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
        let sqsClient = ctx.GetService<IAmazonSQS>()
        
        let! reservationRequestDto = ctx.BindJsonAsync<Dto.ReservationRequest>()
        let reservationRequest = Dto.reservationRequestDtoToDomain reservationRequestDto
        
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        
        let! affectedRows = Data.Reservations.createReservation dbConnStr reservationRequest
        if affectedRows > 0 then
            let! reservationQueue = sqsClient.GetQueueUrlAsync ReservationQueue
            let! _ = sqsClient.SendMessageAsync(reservationQueue.QueueUrl, $"{reservationRequestDto.Id}")
            ()
        transaction.Complete()
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
