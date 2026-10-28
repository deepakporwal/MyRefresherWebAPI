using Microsoft.AspNetCore.Authentication;
using AutoMapper;
using MyRefresherWebAPI;
using MyRefresherWebAPI.Profiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(_ => { }, typeof(RegionProfile));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "MyRefresherWebAPI API", Version = "v1" });
    options.AddSecurityDefinition("basic", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "basic",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Enter the username and password for HTTP Basic Authentication."
    });
    options.AddSecurityRequirement(document => new()
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("basic", document)] = []
    });
});
builder.Services
    .AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);
builder.Services.AddAuthorization();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
