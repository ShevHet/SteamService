// DTOs/CalendarMonthDto.cs
namespace SteamService.DTOs
{
    public class CalendarDayDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class CalendarMonthDto
    {
        public string Month { get; set; } = string.Empty; // "2025-11"
        public List<CalendarDayDto> Days { get; set; } = new();
    }
}
