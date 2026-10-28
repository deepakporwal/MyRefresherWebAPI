using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyRefresherWebAPI.Configuration;
using MyRefresherWebAPI;
using MyRefresherWebAPI.Profiles;
using MyRefresherWebAPI.Services;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, loggerConfiguration) =>
{
    loggerConfiguration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "MyRefresherWebAPI");

    if (context.HostingEnvironment.IsDevelopment())
    {
        loggerConfiguration.WriteTo.Console(
            theme: AnsiConsoleTheme.Code,
            outputTemplate:
                "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {SourceContext}{NewLine}" +
                "  {Message:lj}{NewLine}{Exception}");
    }
    else
    {
        loggerConfiguration.WriteTo.Console(new CompactJsonFormatter());
    }
});

// Add services to the container.

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("ModelValidation");

            logger.LogWarning(
                "Request validation failed. Method {Method} Path {Path} ErrorCount {ErrorCount} TraceId {TraceId}",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path,
                context.ModelState.ErrorCount,
                context.HttpContext.TraceIdentifier);

            return new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));
        };
    });
builder.Services.AddAutoMapper(_ => { }, typeof(RegionProfile));
builder.Services
    .AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services
    .AddOptions<DevelopmentUsersOptions>()
    .BindConfiguration(DevelopmentUsersOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "MyRefresherWebAPI API", Version = "v1" });
    options.AddSecurityDefinition("bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Enter a JWT access token."
    });
    options.AddSecurityRequirement(document => new()
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtConfiguration = builder.Configuration.GetSection(JwtOptions.SectionName);
        var signingKey = jwtConfiguration[ nameof(JwtOptions.SigningKey) ] ?? string.Empty;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtConfiguration[nameof(JwtOptions.Issuer)],
            ValidateAudience = true,
            ValidAudience = jwtConfiguration[nameof(JwtOptions.Audience)],
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearerEvents");

                logger.LogDebug(
                    "JWT validated for user {User} on path {Path}.",
                    context.Principal?.Identity?.Name ?? "unknown",
                    context.HttpContext.Request.Path);

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearerEvents");

                logger.LogWarning(
                    context.Exception,
                    "JWT authentication failed on path {Path}.",
                    context.HttpContext.Request.Path);

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Regions.Read", policy =>
        policy.RequireRole("Admin", "Reader", "Editor"));
    options.AddPolicy("Regions.Write", policy =>
        policy.RequireRole("Admin", "Editor"));
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler(exceptionApp => exceptionApp.Run(async context =>
{
    var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    logger.LogError(
        exceptionFeature?.Error,
        "Unhandled exception while processing request. Method {Method} Path {Path} TraceId {TraceId}",
        context.Request.Method,
        context.Request.Path,
        context.TraceIdentifier);

    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/problem+json";

    var problemDetails = new ProblemDetails
    {
        Status = StatusCodes.Status500InternalServerError,
        Title = "An unexpected error occurred.",
        Instance = context.Request.Path
    };
    problemDetails.Extensions["traceId"] = context.TraceIdentifier;

    await context.Response.WriteAsJsonAsync(problemDetails);
}));
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
        diagnosticContext.Set("User", httpContext.User.Identity?.Name ?? "anonymous");
    };
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
