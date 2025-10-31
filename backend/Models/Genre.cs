using System.ComponentModel.DataAnnotations;

namespace SteamService.Models;

public class Genre
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Навигационные свойства
    public ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}