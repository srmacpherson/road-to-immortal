using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoadToImmortal.Api.Models;

public class Match
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long MatchId { get; set; }

    public long SteamId { get; set; }

    public int PlayerSlot { get; set; }

    public bool RadiantWin { get; set; }

    public int Duration { get; set; }

    public int GameMode { get; set; }

    public int HeroId { get; set; }

    public int Kills { get; set; }

    public int Deaths { get; set; }

    public int Assists { get; set; }

    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}