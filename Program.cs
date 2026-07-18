using System.Security.Cryptography;
using System.Text;
using devalut.Data.Configuration;
using devalut.Entities.Persistance;
using devalut.Interfaces;
using devalut.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using devalut.Models;
using static devalut.Models.User;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.RateLimiting;
using devalut.Models.Enums;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DevaultDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.Configure<CryptoSettings>(builder.Configuration.GetSection("CryptoSettings"));
builder.Services.Configure<JwtService>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddSingleton<IEncryptService, AesGsmEncryptService>();
builder.Services.AddScoped<ITokenService, JwtService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
     ValidateActor = true,
     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]
                                                 ?? throw new KeyNotFoundException("SecretKey no encontrada."))),
     ValidIssuer = builder.Configuration["Jwt:Issuer"],
     ValidateIssuer = true,
     ValidateLifetime = true,
     ValidateAudience = false   
    };
});
builder.Services.AddRateLimiter(options=> 
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("PerUser", context =>
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if(string.IsNullOrEmpty(userId))
            userId = context.Connection.RemoteIpAddress?.ToString();
        
        return RateLimitPartition.GetFixedWindowLimiter(
               partitionKey: userId,
               factory: _=> new FixedWindowRateLimiterOptions
               {
                   PermitLimit = 10,
                   Window =  TimeSpan.FromSeconds(10),
                   QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                   QueueLimit= 2
               }
        );
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy=> policy.RequireRole(nameof(Roles.Admin)));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
