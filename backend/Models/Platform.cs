using System.ComponentModel.DataAnnotations;

namespace SteamService.Models;

public class Platform
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    // Навигационные свойства
    public ICollection<GamePlatform> GamePlatforms { get; set; } = new List<GamePlatform>();

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}