# MyRefresherWebAPI

A small ASP.NET Core Web API targeting .NET 10. The project demonstrates protected region endpoints, HTTP Basic Authentication, AutoMapper, custom model validation, and OpenAPI/Swagger documentation.

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

The API uses HTTP Basic Authentication. The current development credentials are:

- Username: `admin`
- Password: `password`

> These credentials are hard-coded for demonstration purposes only. Replace the authentication implementation with a secure, configurable identity solution before deploying to a real environment.

## API endpoint

### Get regions

```http
GET /api/Region/regions
Authorization: Basic <base64(username:password)>
```

Example using PowerShell:

```powershell
$credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes('admin:password'))
Invoke-RestMethod `
  -Uri 'https://localhost:7156/api/Region/regions' `
  -Headers @{ Authorization = "Basic $credentials" }
```

The endpoint returns the sample regions as JSON:

```json
[
  { "id": 1, "name": "Region 1", "code": "R1" },
  { "id": 2, "name": "Region 2", "code": "R2" },
  { "id": 3, "name": "Region 3", "code": "R3" }
]
```

Requests without valid credentials receive `401 Unauthorized`.

### Add a region

```http
POST /api/Region/regions
Authorization: Basic <base64(username:password)>
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

`BasicAuthenticationHandler` validates the HTTP Basic Authentication header, and `[Authorize]` protects the region controller. Authentication must run successfully before either region endpoint can be called.

## Project structure

- `Program.cs` - Application configuration and middleware pipeline
- `BasicAuthenticationHandler.cs` - Development HTTP Basic Authentication handler
- `Controllers/RegionController.cs` - Protected regions endpoint
- `Models/` - Domain and DTO models
- `Profiles/RegionProfile.cs` - AutoMapper configuration
- `Validation/GreaterThanZeroAttribute.cs` - Custom `id` validation attribute
- `MyRefresherWebAPI.http` - HTTP request examples

## Build

```powershell
dotnet build
```
