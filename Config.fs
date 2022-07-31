module Config

open Microsoft.AspNetCore.Http
open Giraffe
open Microsoft.Extensions.Configuration


type IConfiguration with
    member this.DbConnectionString = this.["DbConnectionString"]
    member this.SqsConnectionString = this.["SqsConnectionString"]


type HttpContext with
    member ctx.Config =
        ctx.GetService<IConfiguration>()

