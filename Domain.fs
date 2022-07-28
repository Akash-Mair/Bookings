module Domain

open System

type LocationId = LocationId of Guid 
type LocationName = LocationName of string

type Location =
    {
        Name: LocationName
        Id: LocationId
    }

type ReservationId = ReservationId of Guid
type ReservationStatus =
    | Requested
    | Processing
    | Successful
    | Failed
    
    member this.Serialise () =
        match this with
        | Requested -> "Requested"
        | Processing -> "Processing"
        | Successful -> "Successful"
        | Failed -> "Failed"
        
    static member Deserialise = function
        | "Requested" -> Requested
        | "Processing" -> Processing
        | "Successful" -> Successful
        | "Failed" -> Failed
        | unknownStatus -> failwithf $"Unknown status: {unknownStatus}"

type Reservation =
    {
        Id: ReservationId
        Date: DateOnly
        Time: TimeOnly
        Seats: int
        SpecialRequest: string option
        Status: ReservationStatus
        Location: LocationId
    }

type BookingId = BookingId of Guid

type Booking =
    {
        Id: BookingId
        Date: DateOnly
        Time: TimeOnly
        Seats: int
        SpecialRequest: string option
        Location: LocationId 
        Reservation: ReservationId
    }



