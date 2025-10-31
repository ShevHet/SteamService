using System.ComponentModel.DataAnnotations;

namespace SteamService.Models;

public class Game
{
    public Guid Id { get; set; }
    public long? SteamAppId { get; set; }   // nullable, если парсинг не через steam id
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;        // SEO-friendly
    public string ShortDescription { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
    public DateTime? ReleaseDate { get; set; }
    public decimal? Price { get; set; }
    public int? MetacriticScore { get; set; }
    public List<string> Genres { get; set; } = new();
    public List<string> Platforms { get; set; } = new();
    public string? StoreUrl { get; set; }           // URL страницы в Steam
    public string? HeaderImageUrl { get; set; }      // URL главного изображения
    public bool IsComingSoon { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
