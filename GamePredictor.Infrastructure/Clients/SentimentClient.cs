using System.Text;
using System.Text.Json;
using GamePredictor.Application.Interfaces;
using GamePredictor.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GamePredictor.Infrastructure.Clients;

public class SentimentClient : ISentimentClient
{
    private readonly HttpClient _httpClient;
    private readonly HuggingFaceOptions _options;
    private readonly ILogger<SentimentClient> _logger;

    public SentimentClient(HttpClient httpClient, IOptions<HuggingFaceOptions> options, ILogger<SentimentClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<double> GetSentimentAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        try
        {
            var payload = new { inputs = text };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
            var response = await _httpClient.PostAsync("https://api-inference.huggingface.co/models/distilbert-base-uncased-finetuned-sst-2-english", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("HuggingFace API вернул ошибку: {StatusCode}", response.StatusCode);
                return 0;
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<SentimentResult>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result != null && result.Count > 0)
            {
                var first = result[0];
                return first.Label == "POSITIVE" ? first.Score : -first.Score;
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка сентимент-анализа");
            return 0;
        }
    }
}

public class SentimentResult
{
    public string Label { get; set; } = string.Empty;
    public double Score { get; set; }
}