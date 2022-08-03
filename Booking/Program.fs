open Amazon.Runtime
open Amazon.SQS
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe.EndpointRouting
open Amazon.Extensions.NETCore.Setup

let endpoints =
    [
        GET [
            route "/locations" Locations.getAllLocations
            routef "/locations/%s" Locations.getLocation
            
            route "/reservations" Reservations.getAllReservations
            routef "/reservations/%s" Reservations.getReservationById
            
            route "/bookings" Bookings.getAllBookings
            routef "/bookings/%s" Bookings.getBookingById
        ]
        POST [
            route "/bookings" Bookings.requestBooking
            route "/bookings/check" Bookings.getAllBookingsByDateBeforeTime
            route "/reservations" Reservations.requestReservation
        ]
    ]

let notFoundHandler =
    "Not Found"
    |> text
    |> RequestErrors.notFound

let configureApp (appBuilder : IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .UseGiraffe(endpoints)
        .UseGiraffe(notFoundHandler)

let configureServices (services : IServiceCollection) =
    let config = services.BuildServiceProvider().GetService<IConfiguration>()
    let serviceUrl = config.GetValue<string>("AWS:ServiceURL")
    services
//        .AddSingleton<IAmazonSQS>(
//            let config = AmazonSQSConfig(ServiceURL=serviceUrl)
//            new AmazonSQSClient(BasicAWSCredentials("temp", "temp"), config))
// TODO: Add an if prod env then use the below commented lines rather than the above
        .AddDefaultAWSOptions(config.GetAWSOptions("AWS"))
        .AddAWSService<IAmazonSQS>()
        .AddRouting()
        .AddGiraffe()
    |> ignore

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services

    let app = builder.Build()

    if app.Environment.IsDevelopment() then
        app.UseDeveloperExceptionPage() |> ignore
    
    configureApp app
    app.Run()
    
    0
