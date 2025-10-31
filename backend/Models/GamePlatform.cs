namespace SteamService.Models;

public class GamePlatform
{
    public Guid GameId { get; set; }
    public int PlatformId { get; set; }

    // ������������� ��������
    public Game Game { get; set; } = null!;
    public Platform Platform { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
}