module Config

open Microsoft.Extensions.Configuration

type IConfiguration with
    member this.DbConnectionString = this.["DbConnectionString"]



