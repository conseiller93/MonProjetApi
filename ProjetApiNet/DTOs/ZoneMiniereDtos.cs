using System.ComponentModel.DataAnnotations;

namespace ProjetApiNet.DTOs;

public class ZoneMiniereDto
{
    public int Id { get; set; }
    public required string Nom { get; set; }
    public decimal DistanceDepotZone { get; set; }
    public decimal DistanceAllerRetour { get; set; }
    public int ToursMaxParJour { get; set; }
    public decimal Tarification { get; set; }
    public int ChargementsMaxParMois { get; set; }
}

public class ZoneMiniereCreateDto
{
    [Required(ErrorMessage = "Le nom de la zone est obligatoire.")]
    [MaxLength(150)]
    public required string Nom { get; set; }
}

public class ZoneMiniereUpdateDto
{
    [MaxLength(150)]
    public string? Nom { get; set; }
}