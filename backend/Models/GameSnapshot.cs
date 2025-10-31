using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamService.Models;

public class GameSnapshot
{
    public int Id { get; set; }
    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;
    public DateTime SnapshotDateUtc { get; set; }
    public long FollowersCount { get; set; }
    public string RawData { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
}