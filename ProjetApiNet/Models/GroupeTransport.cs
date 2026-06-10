using System.ComponentModel.DataAnnotations;

namespace ProjetApiNet.Models  // CORRIGÉ : namespace unifié (plus de TCA.API.Models)
{
    public class GroupeTransport
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nom { get; set; } = null!;

        // Chaque groupe a un superviseur de groupe (salaire : 9 000 000 GNF/mois)
        public int SuperviseurGroupeId { get; set; }
        public Utilisateur SuperviseurGroupe { get; set; } = null!;

        // Chaque groupe est affecté à une zone minière (2 groupes par zone)
        public int ZoneMiniereId { get; set; }
        public ZoneMiniere ZoneMiniere { get; set; } = null!;

        // Chaque groupe contient 10 camions (100 camions / 10 groupes)
        public ICollection<Camion> Camions { get; set; } = new List<Camion>();
    }
}
