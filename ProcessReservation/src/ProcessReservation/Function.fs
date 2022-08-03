namespace ProcessReservation

open Amazon.Lambda.Core
open Amazon.Lambda.SQSEvents
open FsHttp
open System

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer>)>]
()

module Lambda =
    
    let getReservation id =
        http {
            GET $"https://localhost:5001/reservations/{id}"
        }
        |> Request.sendTAsync
    
    let getLocation id =
        http {
            GET $"https://localhost:5001/locations/{id}"
        }
        |> Request.sendTAsync
    
    let getBookingsDateTime (date: string) (time: string) =
        http {
           POST "https://localhost:5001/bookings/check"
           body
           jsonSerialize {| Date = date; Time = time |}
        }
        |> Request.sendTAsync
    
    let createBooking booking =
        http {
            POST "https://localhost:5001/bookings"
            body
            jsonSerialize booking 
        }
        |> Request.sendTAsync
    
    let processRequest (id: string) = task {
        let! reservationRequest = getReservation id
        let! reservation =
            reservationRequest
            |> Response.deserializeJsonTAsync<{| LocationId: Guid; Date: string; Time: string; Seats: int; SpecialRequest: string |}>
        
        let! locationRequest = getLocation reservation.LocationId
        let! location = locationRequest |> Response.deserializeJsonTAsync<{| MaxCapacity: int |}>
        
        let! bookingsRequest = getBookingsDateTime reservation.Date reservation.Time
        let! bookings = bookingsRequest |> Response.deserializeJsonTAsync<{| Seats: int |}[]>
        
        let totalBookings =
            bookings |> Array.sumBy (fun b -> b.Seats)
            
        if totalBookings > location.MaxCapacity then
            return None 
        else
            let bookingRequest =
                {|
                    Id = Guid.NewGuid()
                    Date = reservation.Date
                    Time = reservation.Time
                    Seats = reservation.Seats
                    SpecialRequest = reservation.SpecialRequest
                    LocationId = reservation.LocationId
                    ReservationId = Guid id 
                |}
            let! bookingRequest = createBooking bookingRequest
            let! bookingId = bookingRequest |> Response.deserializeJsonTAsync<string>
            return Some bookingId
    }
    
    let processEvent (sqsEvents: SQSEvent) (ctx: ILambdaContext) = task {
        match sqsEvents.Records |> Seq.tryHead with
        | Some event ->
            return! processRequest event.Body
        | None ->
            return None 
    }