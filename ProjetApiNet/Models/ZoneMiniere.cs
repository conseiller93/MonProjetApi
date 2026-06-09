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

    public int ChargementsMaxParMois { get; set; }

    // Relation fictive ou réelle selon votre architecture (ex: GroupeTransport)
    // public ICollection<GroupeTransport> GroupesTransport { get; set; } = new List<GroupeTransport>();
}
