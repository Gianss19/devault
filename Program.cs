using System.Text;
using System.Text.Json.Serialization;
using devault;
using devault.Data.Configuration;
using devault.Entities.Persistance;
using devault.Interfaces;
using devault.Models;
using devault.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.RateLimiting;
using devault.Models.Enums;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DevaultDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.Configure<CryptoSettings>(builder.Configuration.GetSection("CryptoSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<AdminSeedSettings>(builder.Configuration.GetSection("AdminSeed"));

builder.Services.AddSingleton<IEncryptService, AesGsmEncryptService>();
builder.Services.AddScoped<ITokenService, JwtService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IHasherService, BcryptService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
              {
                  if (builder.Environment.IsDevelopment())
                  {
                      if (string.IsNullOrEmpty(origin)) return true;
                      if (origin == "null") return true;
                  }
                  else
                  {
                      if (string.IsNullOrEmpty(origin)) return false;
                  }
                  try
                  {
                      var uri = new Uri(origin);
                      return uri.Host == "localhost" || uri.Host == "127.0.0.1";
                  }
                  catch { return false; }
              })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:SecretKey"]
            ?? throw new KeyNotFoundException("SecretKey no encontrada."))),
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"]
    };
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("PerUser", context =>
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            userId = context.Connection.RemoteIpAddress?.ToString();

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: userId!,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            });
    });

    options.AddPolicy("Login", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole(nameof(Roles.Admin)));
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        context.Response.ContentType = "application/json";

        if (exception is KeyNotFoundException)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new { error = exception.Message });
        }
        else if (exception is ArgumentException or UnauthorizedAccessException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = exception.Message });
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Error interno del servidor." });
        }
    });
});

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    context.Response.Headers.Append("Content-Security-Policy",
        "frame-ancestors 'none'");
    await next();
});

app.UseCors("AllowFrontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DevaultDbContext>();
    var hasher = services.GetRequiredService<IHasherService>();
    var seedSettings = services.GetRequiredService<IOptions<AdminSeedSettings>>().Value;
    var logger = services.GetRequiredService<ILogger<Program>>();

    await context.Database.MigrateAsync();

    logger.LogInformation("AdminSeed: Email='{Email}', Name='{Name}'", seedSettings.Email, seedSettings.Name);

    var hasAdmin = await context.Users.AnyAsync(u => u.Rol == Roles.Admin);
    logger.LogInformation("AdminSeed: hasAdmin={HasAdmin}", hasAdmin);

    if (!hasAdmin && !string.IsNullOrWhiteSpace(seedSettings.Email))
    {
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == seedSettings.Email);
        if (existingUser != null)
        {
            logger.LogInformation("AdminSeed: Promoting existing user '{Email}' to Admin", seedSettings.Email);
            existingUser.ChangeRole(Roles.Admin);
        }
        else
        {
            logger.LogInformation("AdminSeed: Creating new admin user '{Email}'", seedSettings.Email);
            var passwordHash = hasher.GenerateHash(seedSettings.Password);
            var admin = new User(seedSettings.Name, seedSettings.Email, passwordHash, Roles.Admin);
            context.Users.Add(admin);
        }
        await context.SaveChangesAsync();
        logger.LogInformation("AdminSeed: Saved changes to database");
    }
    else if (hasAdmin)
    {
        logger.LogInformation("AdminSeed: Admin already exists, skipping seed");
    }
    else
    {
        logger.LogWarning("AdminSeed: Email is empty, skipping seed");
    }
}

app.Run();
