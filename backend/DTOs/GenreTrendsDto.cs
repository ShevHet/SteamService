// DTOs/GenreTrendsDto.cs
namespace SteamService.DTOs
{
    public class GenreTrendPointDto
    {
        public string Month { get; set; } = string.Empty; // "2025-10"
        public long? AvgFollowers { get; set; }            // null если данных нет
        public int? Rank { get; set; }                     // null если данных нет
    }

    public class GenreTrendDto
    {
        public string Genre { get; set; } = string.Empty;
        public List<GenreTrendPointDto> Data { get; set; } = new();
    }
}
