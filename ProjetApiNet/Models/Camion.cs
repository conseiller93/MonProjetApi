using System.Collections.Generic;

namespace ProjetApiNet.Models
{
    public class Camion
    {
        public int Id { get; set; }

        public required string Immatriculation { get; set; }

        public required string Modele { get; set; }

        public int ChauffeurId { get; set; }

        public Utilisateur Chauffeur { get; set; } = null!;

        public int GroupeTransportId { get; set; }

        public GroupeTransport GroupeTransport { get; set; } = null!;

        public decimal Kilometrage { get; set; }

        // Relation avec les chargements logistiques
        public ICollection<Chargement> Chargements { get; set; } = new List<Chargement>();
    }
}
