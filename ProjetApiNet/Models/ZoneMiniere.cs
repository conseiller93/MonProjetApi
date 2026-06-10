using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetApiNet.Models;

public class ZoneMiniere
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public required string Nom { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DistanceDepotZone { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DistanceAllerRetour { get; set; }

    public int ToursMaxParJour { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Tarification { get; set; }

    // Chargements max par mois par camion (issu du tableau des primes chauffeurs du PDF)
    public int ChargementsMaxParMois { get; set; }

    // Relations — décommentées car utilisées dans ApplicationDbContext et GroupeTransportRepository
    public ICollection<GroupeTransport> GroupesTransport { get; set; } = new List<GroupeTransport>();
    public ICollection<Chargement> Chargements { get; set; } = new List<Chargement>();
}
