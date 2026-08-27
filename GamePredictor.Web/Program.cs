using GamePredictor.Application.Interfaces;
using GamePredictor.Application.Options;
using GamePredictor.Application.Services;
using GamePredictor.Domain.Entities;
using GamePredictor.Domain.Interfaces;
using GamePredictor.Infrastructure.Clients;
using GamePredictor.Infrastructure.Data;
using GamePredictor.Infrastructure.Options;
using GamePredictor.Infrastructure.Repositories;
using GamePredictor.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RawgOptions>(builder.Configuration.GetSection("RAWG"));
builder.Services.Configure<YoutubeOptions>(builder.Configuration.GetSection("Youtube"));
builder.Services.Configure<HuggingFaceOptions>(builder.Configuration.GetSection("HuggingFace"));
builder.Services.Configure<PredictionOptions>(builder.Configuration.GetSection("Prediction"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Репозитории
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IPredictionRepository, PredictionRepository>();
builder.Services.AddScoped<IMetricRepository, MetricRepository>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<IDeveloperRepository, DeveloperRepository>();

// Клиенты внешних API
builder.Services.AddHttpClient<RawgClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
})
.AddPolicyHandler(Policy<HttpResponseMessage>
    .Handle<HttpRequestException>()
    .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                   r.StatusCode == System.Net.HttpStatusCode.InternalServerError)
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
builder.Services.AddScoped<IGameSourceClient, RawgClient>();

builder.Services.AddHttpClient<SteamClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(Policy<HttpResponseMessage>
    .Handle<HttpRequestException>()
    .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                   r.StatusCode == System.Net.HttpStatusCode.InternalServerError)
    .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
builder.Services.AddScoped<ISteamClient, SteamClient>();

builder.Services.AddHttpClient<YoutubeClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddScoped<IYoutubeClient, YoutubeClient>();

builder.Services.AddHttpClient<SentimentClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddScoped<ISentimentClient, SentimentClient>();

builder.Services.AddHttpClient<RssNewsClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<INewsApiClient, RssNewsClient>();

// Сервисы бизнес-логики
builder.Services.AddScoped<IGenreStatsService, GenreStatsService>();
builder.Services.AddScoped<IPredictionService, PredictionService>();
builder.Services.AddScoped<IDataUpdateService, DataUpdateService>();

// Фоновый сервис
builder.Services.AddHostedService<DataUpdateWorker>();

// 7. Контроллеры, Swagger и Razor Pages
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Инициализация БД: только Unknown
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Developers.Any(d => d.Name == "Unknown"))
    {
        db.Developers.Add(new Developers
        {
            Name = "Unknown",
            AvgMetacriticLast3 = 70,
            GamesCount = 0
        });
        await db.SaveChangesAsync();
    }
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapRazorPages();
app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("Приложение успешно запущено");
});

app.Run();
