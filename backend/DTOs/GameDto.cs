// DTOs/GameDto.cs
using System.Text.Json.Serialization;

namespace SteamService.DTOs
{
    public class GameDto
    {
        public Guid Id { get; set; }
        public long? SteamAppId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? ShortDescription { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public decimal? Price { get; set; }
        public int? MetacriticScore { get; set; }
        public List<string> Genres { get; set; } = new();
        public List<string> Platforms { get; set; } = new();
        public string? StoreUrl { get; set; }
        public string? HeaderImageUrl { get; set; }
        public bool IsComingSoon { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
