namespace GamePredictor.Infrastructure.Clients.DTOs;

public class IgdbGameDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? FirstReleaseDate { get; set; }
    public List<GenreDto>? Genres { get; set; }
    public List<InvolvedCompanyDto>? InvolvedCompanies { get; set; }
    public List<PlatformDto>? Platforms { get; set; }
    public CoverDto? Cover { get; set; }
    public string? Summary { get; set; }
    public double? Rating { get; set; }
}

public class GenreDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class InvolvedCompanyDto
{
    public CompanyDto Company { get; set; } = new();
}

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class PlatformDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CoverDto
{
    public string Url { get; set; } = string.Empty;
}