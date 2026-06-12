using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Projektask.Application.Interfaces;
using Projektask.Application.Options;
using Projektask.Application.Services;
using Projektask.Application.Validators;
using Projektask.Infrastructure.Data;
using Projektask.Infrastructure.Repositories;
using Serilog;

// Konfigurasi Serilog sejak awal agar error startup pun ter-log
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Ganti logging bawaan dengan Serilog
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console());

    // Connection string: appsettings.Development.json (lokal) atau env var ConnectionStrings__Default (Render)
    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' not configured.");

    // JWT secret dari appsettings atau env var JWT_SECRET
    var jwtSecret = builder.Configuration["Jwt:Secret"]
        ?? Environment.GetEnvironmentVariable("JWT_SECRET")
        ?? throw new InvalidOperationException("JWT secret not configured.");

    var jwtExpiresInHours = builder.Configuration.GetValue<int>("Jwt:ExpiresInHours", 24);

    // DbContext (EF Core) — dipakai untuk write/CUD
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(connectionString));

    // JwtSettings disimpan di DI agar bisa dipakai AuthService tanpa coupling ke IConfiguration
    builder.Services.AddSingleton(new JwtSettings(jwtSecret, jwtExpiresInHours));

    // Inject koneksi Dapper — IDbConnection di-resolve per-request (Scoped)
    builder.Services.AddScoped<System.Data.IDbConnection>(_ =>
        new Npgsql.NpgsqlConnection(connectionString));

    // Repository & Service — mirip binding di Laravel service container (App::bind)
    builder.Services.AddScoped<IUserReadRepository, UserReadRepository>();
    builder.Services.AddScoped<IUserWriteRepository, UserWriteRepository>();
    builder.Services.AddScoped<IProjectReadRepository, ProjectReadRepository>();
    builder.Services.AddScoped<IProjectWriteRepository, ProjectWriteRepository>();
    builder.Services.AddScoped<ITaskReadRepository, TaskReadRepository>();
    builder.Services.AddScoped<ITaskWriteRepository, TaskWriteRepository>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IProjectService, ProjectService>();
    builder.Services.AddScoped<ITaskService, TaskService>();

    // Supaya User.FindFirst("sub") bekerja — tanpa ini JwtBearer memetakan "sub" ke ClaimTypes.NameIdentifier
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

    // JWT Authentication — mirip guard di Laravel
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret))
            };

            // Kembalikan format error kontrak bagian 4 saat token tidak valid / tidak ada
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

    // FluentValidation — daftarkan semua validator dari assembly Application
    builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
    builder.Services.AddFluentValidationAutoValidation();

    // Format error validasi sesuai kontrak bagian 4: { message, errors: { field: [msg] } }
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

    // CORS: izinkan origin dari env var ALLOWED_ORIGINS (koma-separated), misal domain Vercel
    var allowedOrigins = (builder.Configuration["AllowedOrigins"]
        ?? Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
        ?? "http://localhost:3000")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    builder.Services.AddCors(opt =>
        opt.AddDefaultPolicy(policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()));

    // Swagger — aktif di semua environment agar reviewer bisa membukanya
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "ProjekTask API",
            Version = "v1",
            Description = "Backend API untuk aplikasi manajemen Project & Task — Sarastya Project-Based Test"
        });

        // Tombol Authorize di Swagger UI untuk input Bearer token
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
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // Global exception handler (.NET 8) — format error sesuai kontrak bagian 4
    builder.Services.AddExceptionHandler<Projektask.Api.Middleware.GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // Jalankan migrasi otomatis saat startup — supaya deploy Render langsung siap tanpa manual migration
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjekTask API v1"));

    app.MapControllers();

    // Health check untuk Render
    app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // HostAbortedException dilempar oleh dotnet-ef saat membaca DbContext — bukan error sungguhan
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
