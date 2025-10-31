namespace SteamService.Models;

public class GameGenre
{
    public Guid GameId { get; set; }
    public int GenreId { get; set; }

    // ������������� ��������
    public Game Game { get; set; } = null!;
    public Genre Genre { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
}