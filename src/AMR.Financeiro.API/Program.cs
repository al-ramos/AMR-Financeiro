using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AMR.Financeiro.Application;
using AMR.Financeiro.API.Middleware;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Infrastructure;
using AMR.Financeiro.Infrastructure.Data;
using AMR.Financeiro.Shared.Security;
using Serilog;

// ── Serilog — configuração antecipada para capturar erros de startup ──────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "AMR.Financeiro.API")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// ── Serilog como provider de log ──────────────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "AMR.Financeiro.API")
    .WriteTo.Console(
        outputTemplate: ctx.HostingEnvironment.IsProduction()
            ? "[{Timestamp:o} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}"
            : "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));

// ── Infraestrutura + Application ──────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── JWT Authentication ─────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key não configurado em appsettings.json");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "AMR.Financeiro",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "AMR",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();

// ── Rate Limiting — 100 req/min por IP ────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }));
    options.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.Headers.RetryAfter = "60";
        await ctx.HttpContext.Response.WriteAsync("Too many requests. Retry after 60 seconds.", ct);
    };
});

// ── Controllers + Swagger com Bearer ──────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AMR.Financeiro API", Version = "v1" });

    // Habilita Bearer no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Informe: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
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

// ── CORS para o frontend ───────────────────────────────────────────────────────
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3001")
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

// ── Migrations + Seed ──────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<FinanceiroDbContext>();
    await ctx.Database.MigrateAsync();
    await PlanoContasSeed.AplicarAsync(ctx, cdFilial: 1);
    await LancamentosDemoSeed.AplicarAsync(ctx, cdFilial: 1);

    // Cria usuário admin padrão se não existir — SQL direto para evitar EF Core 9 + SQLite sentinel bug
    var adminExists = await ctx.Usuarios.AnyAsync(u => u.Username == "admin");
    if (!adminExists)
    {
        var hash = PasswordHelper.Hash("admin123");
        var now  = DateTime.UtcNow.ToString("o");
        await ctx.Database.ExecuteSqlRawAsync($@"
            INSERT INTO ""Usuarios"" (""Username"", ""PasswordHash"", ""Role"", ""CriadoEm"")
            VALUES ('admin', '{hash}', 'Admin', '{now}')
        ");
        app.Logger.LogInformation("Usuário admin criado com senha padrão: admin123");
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger habilitado em todos os ambientes (projeto interno)
// RoutePrefix = "api/swagger" para funcionar atrás do ALB (que roteia /api/* para este container)
app.UseSwagger(c => c.RouteTemplate = "api/swagger/{documentName}/swagger.json");
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "AMR.Financeiro API v1");
    c.RoutePrefix = "api/swagger";
});

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

// Redirect raiz para Swagger em dev (facilita preview e testes locais)
if (app.Environment.IsDevelopment())
    app.MapGet("/", () => Results.Redirect("/api/swagger")).ExcludeFromDescription();

// ── Serilog request logging ───────────────────────────────────────────────────
app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} → {StatusCode} ({Elapsed:0.000}ms)";
});

// ── Security Headers (OWASP) ──────────────────────────────────────────────────
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"]  = "nosniff";
    ctx.Response.Headers["X-Frame-Options"]         = "DENY";
    ctx.Response.Headers["X-XSS-Protection"]        = "1; mode=block";
    ctx.Response.Headers["Referrer-Policy"]         = "strict-origin-when-cross-origin";
    ctx.Response.Headers["Permissions-Policy"]      = "geolocation=(), microphone=(), camera=()";
    if (!ctx.Request.IsHttps && app.Environment.IsProduction())
        ctx.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    await next();
});

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
