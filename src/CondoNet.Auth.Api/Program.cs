using CondoNet.Auth.Api.Endpoints;
using CondoNet.Auth.Api.Middleware;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Auth.Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("CondoNet.Auth.Infrastructure"))); // Las migraciones viven aquí

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "CondoNet Auth API",
        Version = "v1",
        Description = "API para la autenticación de CondoNet",
        Contact = new Microsoft.OpenApi.OpenApiContact
        {
            Name = "CondoNet Support Team",
            Email = "suportTeam@condonet.net",

        }
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    s.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    s.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Escribe el token JWT directamente: [Tu_Token]"
    });

    s.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-Api-Key", // Nombre del header que espera tu API
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Ingresa tu API Key en el header X-Api-Key"
    });

    s.AddSecurityRequirement(d => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", d)] = [],
        [new OpenApiSecuritySchemeReference("ApiKey", d)] = []
    });
});
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var settings = builder.Configuration.GetSection("RabbitMqSettings");

        // "rabbitmq" es el nombre del servicio en tu docker-compose
        cfg.Host(settings["Host"], "/", h =>
        {
            h.Username(settings["Username"] ?? "guest");
            h.Password(settings["Password"] ?? "guest");
        });
    });
});

var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:Key"]!);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false; // cambiar a true en producción
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddScoped<IIdentityService, IdentityService>();

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CondoNet Auth API V1");
        c.RoutePrefix = string.Empty; // Esto hace que Swagger cargue en la raíz (http://localhost:XXXX/)
    });
}

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/swagger"))
    {
        // Ejemplo simple: verifica un header de autorización
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Swagger\"");
            context.Response.StatusCode = 401;
            return;
        }
    }
    await next();
});


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapAuthEndpoints();
app.MapApiKeyEndpoints();
app.MapPasswordEndpoints();
app.MapUserEndpoints();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
