using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Avans.Identity.Dapper;
using ZorgAppAPI.Services;
using ZorgAppAPI.Repositories;
using System.Data;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Services.AddLogging();

// Add Identity Framework with Dapper (correct configuration order)
builder.Services
    .AddAuthorization()
    .AddIdentityApiEndpoints<IdentityUser>()
    .AddDapperStores(options =>
        options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection"));

// Add HttpContextAccessor for accessing the current user
builder.Services.AddHttpContextAccessor();

// Register the AuthenticationService
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Register the repositories
builder.Services.AddScoped<ITrajectRepository, TrajectRepository>();
builder.Services.AddScoped<IZorgmomentRepository, ZorgmomentRepository>();

// Register the database connection
builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Bearer token options
builder.Services
    .AddOptions<BearerTokenOptions>()
    .Bind(builder.Configuration.GetSection("BearerToken"))
    .Configure(options =>
    {
        options.BearerTokenExpiration = TimeSpan.FromHours(1);
        options.RefreshTokenExpiration = TimeSpan.FromDays(7);
    });

// Add controllers for the API
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger UI configuration (only in development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirection and authorization middleware
app.UseHttpsRedirection();
app.UseAuthentication();  // Authentication middleware
app.UseAuthorization();   // Authorization middleware

// API Endpoints
app.MapGet("/", () => "API is up");

// Map Identity API endpoints with correct syntax for ASP.NET Core 8
app.MapGroup("account")
   .MapIdentityApi<IdentityUser>();

// Logout endpoint
app.MapPost("/account/logout", async (SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
});

app.MapControllers();

app.Run();
