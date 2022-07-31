module Domain

open System

type LocationId =
    | LocationId of Guid
    member this.Value = match this with LocationId id -> id 
    
type LocationName = LocationName of string

type Location =
    {
        Name: LocationName
        Id: LocationId
    }

type ReservationId =
    | ReservationId of Guid
    
    member this.Value = match this with ReservationId id -> id 
    
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
        LocationId: LocationId
        CreatedOn: DateTime
    }
    
    member this.Serialise () =
        {|
            Id = this.Id.Value
            Date = this.Date.ToShortDateString()
            Time = this.Time.ToShortTimeString()
            Seat = this.Seats
            SpecialRequest = this.SpecialRequest |> Option.defaultValue String.Empty
            LocationId = this.LocationId.Value
            CreatedOn = this.CreatedOn.ToString()
            Status = this.Status.Serialise ()
        |}

type ReservationRequest =
       {
        Id: ReservationId
        Date: DateOnly
        Time: TimeOnly
        Seats: int
        SpecialRequest: string option
        Status: ReservationStatus
        LocationId: LocationId
    } 

type BookingId =
    | BookingId of Guid
    member this.Value = match this with BookingId id -> id 

type Booking =
    {
        Id: BookingId
        Date: DateOnly
        Time: TimeOnly
        Seats: int
        SpecialRequest: string option
        LocationId: LocationId 
        ReservationId: ReservationId
        CreatedOn: DateTime
    }
    
    member this.Serialise () =
        {|
            Id = this.Id.Value
            Date = this.Date.ToShortDateString()
            Time = this.Time.ToShortTimeString()
            Seat = this.Seats
            SpecialRequest = this.SpecialRequest |> Option.defaultValue String.Empty
            LocationId = this.LocationId.Value
            CreatedOn = this.CreatedOn.ToString()
            ReservationId = this.ReservationId.Value
        |}

type BookingRequest =
    {
        Id: BookingId
        Date: DateOnly
        Time: TimeOnly
        Seats: int
        SpecialRequest: string option
        LocationId: LocationId 
        ReservationId: ReservationId
    }



