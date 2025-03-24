using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Avans.Identity.Dapper;
using ZorgAppAPI.Services;
using ZorgAppAPI.Repositories;
using System.Data;
using Microsoft.Data.SqlClient;
using ZorgAppAPI.Interfaces;

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
builder.Services.AddScoped<IAuthenticationService, AspNetIdentityAuthenticationService>();

// Register the repositories
builder.Services.AddScoped<ITrajectRepository, TrajectRepository>();
builder.Services.AddScoped<IZorgmomentRepository, ZorgmomentRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IArtsRepository, ArtsRepository>();
builder.Services.AddScoped<IOuderVoogdRepository, OuderVoogdRepository>();
builder.Services.AddScoped<INotitieRepository, NotitieRepository>();
builder.Services.AddScoped<IUserZorgMomentRepository, UserZorgMomentRepository>();


// Register the database connection
builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();

// Configure Bearer token options
builder.Services
    .AddOptions<BearerTokenOptions>()
    .Bind(builder.Configuration.GetSection("BearerToken"))
    .Configure(options =>
    {
        options.BearerTokenExpiration = TimeSpan.FromHours(1);
        options.RefreshTokenExpiration = TimeSpan.FromDays(7);
    });

// Configureer Identity zonder EF
builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>();

// Add controllers for the API
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Move these lines before builder.Build()
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IAuthenticationService, AspNetIdentityAuthenticationService>();


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

app.MapControllers()
    .RequireAuthorization();

app.Run();
