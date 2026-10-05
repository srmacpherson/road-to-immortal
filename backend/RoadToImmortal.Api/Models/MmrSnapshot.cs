using System.ComponentModel.DataAnnotations;

namespace RoadToImmortal.Api.Models;

public class MmrSnapshot
{
    [Key]
    public int Id { get; set; }

    public long SteamId { get; set; }

    public int Mmr { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Was this snapshot confirmed by the user (true) or estimated/automatic (false)?
    public bool IsConfirmed { get; set; } = false;
}