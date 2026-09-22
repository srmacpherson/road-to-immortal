using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoadToImmortal.Api.Models;

public class Player
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long SteamId { get; set; }

    public string PersonaName { get; set; } = string.Empty;

    public int? CurrentMmr { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}