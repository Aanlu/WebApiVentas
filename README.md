# WebApiVentas

A C# Web API for a sales domain, built with ASP.NET Core on .NET 10.

## What it includes

- Controllers for products, fruits, vegetables, meats, authentication and a
  base product resource.
- Models for products and users.
- A socket-based service utility and a task manager utility.
- OpenAPI support through `Microsoft.AspNetCore.OpenApi`.
- SQL Server data access through `Microsoft.Data.SqlClient`.

## Structure

```text
WebApiVentas/
├── Controladores/   API controllers
├── Modelos/         domain models
├── Servicios/       socket service
├── Utilidades/      task manager
├── Program.cs       application entry point
└── appsettings.json configuration
```

## Run locally

Requirements: .NET 10 SDK and access to a configured SQL Server instance.

```sh
dotnet run --project WebApiVentas/WebApiVentas.csproj
```

`WebApiVentas.http` contains example requests for manual testing.
