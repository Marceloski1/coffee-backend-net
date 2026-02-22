using Coffee.Application.Interfaces;
using Coffee.Application.Services;
using Coffee.Application.Options;
using Coffee.Application.Validators;
using Coffee.Application.DTOs;
using Coffee.Persistence.Repositories;
using Coffee.Persistence.Data;
using Coffee.Persistence.UnitOfWork;
using Coffee.Api.Middleware;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Coffee.Api")
    .WriteTo.Console()
    .WriteTo.File("logs/coffee-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Usar Serilog
builder.Host.UseSerilog();

// Configure Options Pattern
builder.Services.Configure<CacheOptions>(
    builder.Configuration.GetSection(CacheOptions.SectionName));

// Add services to the container.
builder.Services.AddDbContext<CoffeeDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
    .EnableDetailedErrors(builder.Environment.IsDevelopment()));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CoffeeDbContext>("database");

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
    };
});

// Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { 
        Title = "Coffee API", 
        Version = "v1",
        Description = "API para gestión de cafés con Clean Architecture, CQRS y mejores prácticas"
    });
    
    // Incluir comentarios XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // Configuración de JWT en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Dependency Injection - Repositories
builder.Services.AddScoped<ICoffeeRepository, CoffeeRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();

// Dependency Injection - Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Dependency Injection - Services
builder.Services.AddScoped<ICoffeeService, CoffeeService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IIngredientService, IngredientService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateCoffeeDtoValidator>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Coffee.Application.AutoMapper.AutoMapperProfile).Assembly);

// Cache Service
var cacheOptions = builder.Configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>();
if (cacheOptions?.UseRedis ?? false)
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = cacheOptions.RedisConnection;
        options.InstanceName = cacheOptions.RedisInstanceName;
    });
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
}
else
{
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheService, CacheService>();
}

var app = builder.Build();

// Middleware de manejo de excepciones
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Coffee API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health Checks
app.MapHealthChecks("/health");

app.MapControllers();

try
{
    Log.Information("Iniciando Coffee API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
