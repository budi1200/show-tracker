using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using NLog.Web;
using ShowTracker.Core.Configuration;
using ShowTracker.Core.Interfaces;
using ShowTracker.Core.Models;
using ShowTracker.Infrastructure.Clients;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Configuration
builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);
builder.Services.AddOptions<TvDbOptions>()
    .Bind(builder.Configuration.GetSection(TvDbOptions.Name))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<ITvDbClient, TvDbClient>((client, serviceProvider) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<TvDbOptions>>().Value;
    var logger = serviceProvider.GetRequiredService<ILogger<TvDbClient>>();

    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    return new TvDbClient(logger, client, options.ApiKey);
});

var app = builder.Build();

var tvDbClient = app.Services.GetRequiredService<ITvDbClient>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Test endpoint
app.MapGet("/test", async () =>
{
    var data = await tvDbClient.GetDataAsync<TvDbArtworkStatus[]>("artwork/statuses", CancellationToken.None);
    logger.LogInformation("{@value}", data);
});

app.Run();