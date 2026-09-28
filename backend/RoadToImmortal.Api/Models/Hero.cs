using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoadToImmortal.Api.Models;

public class Hero
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int HeroId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string LocalizedName { get; set; } = string.Empty;
}