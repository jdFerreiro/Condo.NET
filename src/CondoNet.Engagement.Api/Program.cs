
using CondoNet.Engagement.Api.Endpoints;
using CondoNet.Engagement.Core.Repositories;
using CondoNet.Engagement.Core.Services;
using CondoNet.Engagement.Infrastructure.Persistence;
using CondoNet.Engagement.Infrastructure.Repositories;
using CondoNet.Engagement.Infrastructure.Services;
using CondoNet.Shared.Interfaces;
using CondoNet.Shared.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Add DbContext
builder.Services.AddDbContext<EngagementDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("CondoNet.Engagement.Infrastructure")));


// 3. Inyección de Dependencias
// Repositorios
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IDigitalSignatureRepository, DigitalSignatureRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IQuorumRepository, QuorumRepository>();
builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();


// Servicios
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IDigitalSignatureService, DigitalSignatureService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IQuorumService, QuorumService>();
builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<IVoteService, VoteService>();


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

app.Run();
