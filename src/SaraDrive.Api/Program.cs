using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SaraDrive.Application.Interfaces;
using SaraDrive.Application.Options;
using SaraDrive.Application.Services;
using SaraDrive.Application.Validators;
using SaraDrive.Infrastructure.Data;
using SaraDrive.Infrastructure.Repositories;
using Serilog;

// Configure Serilog first so even startup errors are logged.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console());

    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' not configured.");

    var jwtSecret = builder.Configuration["Jwt:Secret"]
        ?? Environment.GetEnvironmentVariable("JWT_SECRET")
        ?? throw new InvalidOperationException("JWT secret not configured.");

    var jwtExpiresInHours = builder.Configuration.GetValue("Jwt:ExpiresInHours", 24);

    var streamerBase = builder.Configuration["Streamer:BaseUrl"]
        ?? Environment.GetEnvironmentVariable("STREAMER_BASE_URL")
        ?? "https://stream.tncp.web.id";

    // EF Core — WRITE/CUD paths. NO migrations are run: the schema is owned by the umbrella
    // schema.sql (Python engine + web share the tables); EF only maps onto existing tables.
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));

    builder.Services.AddSingleton(new JwtSettings(jwtSecret, jwtExpiresInHours));
    builder.Services.AddSingleton(new StreamerSettings(streamerBase));

    // Dapper connection — resolved per request for READ repositories.
    builder.Services.AddScoped<System.Data.IDbConnection>(_ =>
        new Npgsql.NpgsqlConnection(connectionString));

    // Read repositories (Dapper)
    builder.Services.AddScoped<IUserReadRepository, UserReadRepository>();
    builder.Services.AddScoped<IDriveReadRepository, DriveReadRepository>();
    // Write repositories (EF Core)
    builder.Services.AddScoped<IUserWriteRepository, UserWriteRepository>();
    builder.Services.AddScoped<IFolderWriteRepository, FolderWriteRepository>();
    builder.Services.AddScoped<IItemWriteRepository, ItemWriteRepository>();
    builder.Services.AddScoped<ITagWriteRepository, TagWriteRepository>();
    builder.Services.AddScoped<IUploadWriteRepository, UploadWriteRepository>();
    // Services
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IDriveService, DriveService>();
    builder.Services.AddScoped<IFolderService, FolderService>();
    builder.Services.AddScoped<IItemService, ItemService>();
    builder.Services.AddScoped<ITagService, TagService>();
    builder.Services.AddScoped<IUploadService, UploadService>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opt =>
        {
            // Keep "sub" as "sub" (don't remap to ClaimTypes.NameIdentifier) so User.FindFirst("sub") works.
            opt.MapInboundClaims = false;
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };
            opt.Events = new JwtBearerEvents
            {
                OnChallenge = async ctx =>
                {
                    ctx.HandleResponse();
                    ctx.Response.StatusCode = 401;
                    ctx.Response.ContentType = "application/json";
                    await ctx.Response.WriteAsJsonAsync(
                        new { message = "Token tidak valid atau sudah kedaluwarsa" });
                }
            };
        });
    builder.Services.AddAuthorization();

    builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
    builder.Services.AddFluentValidationAutoValidation();

    // Validation error shape: { message, errors: { field: [msg] } }
    builder.Services.Configure<ApiBehaviorOptions>(opt =>
    {
        opt.InvalidModelStateResponseFactory = ctx =>
        {
            var errors = ctx.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
            return new BadRequestObjectResult(new { message = "Validasi gagal", errors });
        };
    });

    builder.Services.AddControllers();

    var allowedOrigins = (builder.Configuration["AllowedOrigins"]
        ?? Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
        ?? "http://localhost:3000")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    builder.Services.AddCors(opt =>
        opt.AddDefaultPolicy(policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "SaraDrive API",
            Version = "v1",
            Description = "REST API untuk Sarastya Drive — cloud drive berbasis Telegram (auth JWT + metadata CRUD)."
        });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Masukkan token JWT. Contoh: eyJhbGci..."
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddExceptionHandler<SaraDrive.Api.Middleware.GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SaraDrive API v1"));

    app.MapControllers();
    app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
