using System.ComponentModel.DataAnnotations;

namespace TCA.API.Models
{
    public class GroupeTransport
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nom { get; set; } = null!;

        // Chaque groupe a un superviseur de groupe payé à 9.000.000 GNF/mois
        public int SuperviseurGroupeId { get; set; }
        public Utilisateur SuperviseurGroupe { get; set; } = null!;

        // Chaque groupe travaille dans une zone minière
        public int ZoneMiniereId { get; set; }
        public ZoneMiniere ZoneMiniere { get; set; } = null!;

        // Un groupe contient plusieurs camions (Flotte totale TCA de 100 camions)
        public ICollection<Camion> Camions { get; set; } = new List<Camion>();
    }
}
