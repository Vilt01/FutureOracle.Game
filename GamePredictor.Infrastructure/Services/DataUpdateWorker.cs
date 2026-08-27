using GamePredictor.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GamePredictor.Infrastructure.Services;

public class DataUpdateWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DataUpdateWorker> _logger;

    public DataUpdateWorker(IServiceScopeFactory scopeFactory, ILogger<DataUpdateWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DataUpdateWorker запущен.");
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // задержка 30 сек при старте

        while (!stoppingToken.IsCancellationRequested)
        {
            await UpdateDataAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken); // раз в 6 часов
        }
        _logger.LogInformation("DataUpdateWorker остановлен.");
    }

    private async Task UpdateDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dataUpdateService = scope.ServiceProvider.GetRequiredService<IDataUpdateService>();
            var result = await dataUpdateService.UpdateAllDataAsync();
            _logger.LogInformation("Автоматическое обновление выполнено. Игр: {Games}, прогнозов: {Predictions}",
                result.GamesLoaded, result.PredictionsCalculated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при автоматическом обновлении данных");
        }
    }
}
