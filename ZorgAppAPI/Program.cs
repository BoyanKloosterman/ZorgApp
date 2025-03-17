using Microsoft.EntityFrameworkCore;
using ZorgAppAPI.Data;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Avans.Identity.Dapper;
using ZorgAppAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add database context for ZorgAppAPI
builder.Services.AddDbContext<ZorgAppContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Configure Bearer token options
builder.Services
    .AddOptions<BearerTokenOptions>()
    .Bind(builder.Configuration.GetSection("BearerToken"))
    .Configure(options =>
    {
        options.BearerTokenExpiration = TimeSpan.FromHours(1);
        options.RefreshTokenExpiration = TimeSpan.FromDays(7);
    });


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

// Map controllers (authorization required for all endpoints)
app.MapControllers().RequireAuthorization();

app.Run();
