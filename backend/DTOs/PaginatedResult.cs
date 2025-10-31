namespace SteamService.DTOs
{
    public class PaginatedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public long Total { get; set; }
        public IEnumerable<T> Items { get; set; } = new List<T>();
    }
}
