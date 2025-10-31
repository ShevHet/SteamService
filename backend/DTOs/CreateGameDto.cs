namespace SteamService.DTOs
{
	public class CreateGameDto
	{
		public string Title { get; set; } = string.Empty;
		public string? Description { get; set; }
		public string? Genre { get; set; }
		public decimal Price { get; set; }
		public DateTime ReleaseDate { get; set; }
		public string? Developer { get; set; }
		public string? Publisher { get; set; }
		public string? ImageUrl { get; set; }
	}
}
