# MyRefresherWebAPI

A small ASP.NET Core Web API targeting .NET 10. The project demonstrates protected region endpoints, JWT Bearer authentication, AutoMapper, custom model validation, and OpenAPI/Swagger documentation.

## Requirements

- .NET 10 SDK
- An IDE such as Visual Studio 2026, or the .NET CLI

## Run the API

From the repository root:

```powershell
dotnet restore
dotnet run --project MyRefresherWebAPI.csproj --launch-profile https
```

The development profiles use these URLs:

- HTTP: `http://localhost:5012`
- HTTPS: `https://localhost:7156`

When running in the Development environment, Swagger UI is available at:

- `https://localhost:7156/swagger`

## Authentication

The API uses Microsoft JWT Bearer authentication. In the Development environment, the token endpoint accepts the users configured in `appsettings.Development.json`:

| Username | Password | Roles |
| --- | --- | --- |
| `admin` | `password` | `Admin`, `Reader`, `Editor` |
| `reader` | `reader-password` | `Reader` |
| `editor` | `editor-password` | `Reader`, `Editor` |

The development signing key is also intentionally scoped to the local Development configuration. Before deploying, provide a unique signing key and credential values through an external configuration provider or environment variables. Do not commit production secrets.

The JWT settings are:

- `Jwt__SigningKey` - a private signing key of at least 32 characters.
- `Jwt__Issuer` - the expected token issuer.
- `Jwt__Audience` - the expected token audience.
- `Jwt__ExpirationMinutes` - token lifetime from 1 through 1440 minutes.
- `DevelopmentUsers__Users` - the configured development users, passwords, and roles for this sample API.

For example, set production values in PowerShell before starting the API:

```powershell
$env:Jwt__SigningKey = 'replace-with-a-random-production-key'
$env:Jwt__Issuer = 'https://api.example.com'
$env:Jwt__Audience = 'myrefresherwebapi-clients'
$env:DevelopmentUsers__Users__0__Username = 'replace-with-a-managed-username'
$env:DevelopmentUsers__Users__0__Password = 'replace-with-a-managed-password'
$env:DevelopmentUsers__Users__0__Roles__0 = 'Admin'
```

Use a secret manager such as Azure Key Vault in a real deployment rather than placing secrets in shell history or committed configuration.

## Logging

The API uses `Serilog.AspNetCore` through the standard `ILogger<T>` abstraction. In the `Development` environment, logs are formatted as readable, separated, ANSI-colored console entries. Each request is emitted as one summary event with the HTTP method, path, status code, elapsed time, authenticated username, and trace ID. Outside Development, logs use compact JSON for ingestion by log platforms.

Logging covers:

- HTTP request completion through Serilog request logging.
- Unhandled exceptions through the centralized exception handler.
- JWT validation successes and failures.
- Token request acceptance or rejection and token issuance metadata.
- Region and weather operations.
- Model-validation failures with the number of validation errors.

For safety, the application does not log passwords, access tokens, signing keys, authorization headers, JWT claims payloads, or request/response bodies. The development password and signing key shown in this README are for local learning only and must be replaced before deployment.

Serilog packages used by the project are:

- `Serilog.AspNetCore` 10.0.0
- `Serilog.Sinks.Console` 6.1.1
- `Serilog.Formatting.Compact` 3.0.0

The logging provider is configured in `Program.cs`. Development uses a human-readable console template; non-Development environments use `RenderedCompactJsonFormatter` so fields remain searchable and machine-readable.

## API endpoint

### Get regions

First request a token:

```http
POST /api/Auth/token
Content-Type: application/json

{
  "username": "admin",
  "password": "password"
}
```

Then send the returned `accessToken` as a Bearer token:

```http
GET /api/Region/regions
Authorization: Bearer <accessToken>
```

Example using PowerShell:

```powershell
$tokenResponse = Invoke-RestMethod `
  -Uri 'https://localhost:7156/api/Auth/token' `
  -Method Post `
  -ContentType 'application/json' `
  -Body (@{ username = 'admin'; password = 'password' } | ConvertTo-Json)
Invoke-RestMethod `
  -Uri 'https://localhost:7156/api/Region/regions' `
	-Headers @{ Authorization = "Bearer $($tokenResponse.accessToken)" }
```

The endpoint returns the sample regions as JSON:

```json
[
  { "id": 1, "name": "Region 1", "code": "R1" },
  { "id": 2, "name": "Region 2", "code": "R2" },
  { "id": 3, "name": "Region 3", "code": "R3" }
]
```

The token contains one `role` claim for each assigned role. The region policies are:

- `Regions.Read`: `Admin`, `Reader`, or `Editor`; used by `GET /api/Region/regions`.
- `Regions.Write`: `Admin` or `Editor`; used by `POST /api/Region/regions`.

Requests without a valid token receive `401 Unauthorized`. A valid token with insufficient roles receives `403 Forbidden`. For example, `reader` can get regions but receives `403 Forbidden` when adding one, while `editor` can do both.

### Add a region

```http
POST /api/Region/regions
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "id": 4,
  "name": "Region 4",
  "code": "R4"
}
```

The request model is validated automatically by ASP.NET Core. The following rules apply:

- `id` must be greater than zero.
- `name` is required.
- `code` is required.

Invalid requests return `400 Bad Request` with the validation errors. A valid request currently returns `200 OK` with a success message; it does not persist data.

## Features

### AutoMapper

`Profiles/RegionProfile.cs` maps between the `Region` domain model and `RegionDTO`. The `GET` endpoint uses AutoMapper to convert the sample domain regions into DTOs before returning them to the client.

### Custom validation

`Validation/GreaterThanZeroAttribute.cs` defines the `[GreaterThanZero]` validation attribute. It is applied to `RegionDTO.Id` and returns a validation error when the value is zero or negative.

Because `RegionController` uses `[ApiController]`, ASP.NET Core automatically checks model validation results before executing the action. Invalid models are rejected with `400 Bad Request`.

### Authentication and authorization

`AuthController` issues short-lived JWT access tokens from the configured sample credentials. JWT Bearer validation protects the region and weather controllers through `[Authorize]`; tokens must have the configured issuer, audience, signature, and valid lifetime.

## Project structure

- `Program.cs` - Application configuration and middleware pipeline
- `Controllers/AuthController.cs` - Development token endpoint
- `Configuration/` - JWT and credential configuration options
- `Services/JwtTokenService.cs` - JWT creation service
- `Controllers/RegionController.cs` - Protected regions endpoint
- `Models/` - Domain and DTO models
- `Profiles/RegionProfile.cs` - AutoMapper configuration
- `Validation/GreaterThanZeroAttribute.cs` - Custom `id` validation attribute
- `MyRefresherWebAPI.http` - HTTP request examples

## Build

```powershell
dotnet build
```
