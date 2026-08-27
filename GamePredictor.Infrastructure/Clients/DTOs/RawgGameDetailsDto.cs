using System.Text.Json.Serialization;

namespace GamePredictor.Infrastructure.Clients.DTOs;

public class RawgGameDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<RawgDeveloperDto>? Developers { get; set; }
    public List<RawgStoreDto>? Stores { get; set; }
    public RawgClipDto? Clip { get; set; }
}

public class RawgDeveloperDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class RawgStoreDto
{
    public RawgStoreInfo? Store { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class RawgStoreInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class RawgClipDto
{
    public string Url { get; set; } = string.Empty;
}