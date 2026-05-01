
using CondoNet.Engagement.Infrastructure.Persistence;
using CondoNet.Engagement.Core.Repositories;
using CondoNet.Engagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Add DbContext
builder.Services.AddDbContext<EngagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EngagementDb")));

// Register repositories
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();
builder.Services.AddScoped<IQuorumRepository, QuorumRepository>();
builder.Services.AddScoped<IDigitalSignatureRepository, DigitalSignatureRepository>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map minimal endpoints
app.MapAnnouncementEndpoints();
app.MapNotificationEndpoints();
app.MapSurveyEndpoints();
app.MapVoteEndpoints();
app.MapQuorumEndpoints();
app.MapDigitalSignatureEndpoints();

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
