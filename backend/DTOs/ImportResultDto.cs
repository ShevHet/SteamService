namespace SteamService.DTOs
{
    public class ImportResultDto
    {
        public int Added { get; set; }
        public int Updated { get; set; }
        public int Failed { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}
