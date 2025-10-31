// DTOs/GenreStatDto.cs
namespace SteamService.DTOs
{
    public class GenreStatDto
    {
        public string Genre { get; set; } = string.Empty;
        public int Games { get; set; }
        public long AvgFollowers { get; set; }
    }
}
