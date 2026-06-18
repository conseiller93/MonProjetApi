using System.Collections.Generic;

namespace ProjetApiNet.Models
{
    public class Utilisateur
    {
        public int Id { get; set; }

        public required string Identifiant { get; set; }

        public required string MotDePasseHash { get; set; }

        public RoleUtilisateur Role { get; set; }

        // Propriété indispensable pour stocker la rémunération fixe selon le rôle (PDF)
        public decimal SalaireMensuelGNF { get; set; }

        // Confirmation / activation du compte
        public bool IsConfirmed { get; set; }
        public string? ConfirmationToken { get; set; }
        public DateTime? ConfirmationExpires { get; set; }

        // Relations Entity Framework Core
        public ICollection<GroupeTransport> GroupesSupervises { get; set; } = new List<GroupeTransport>();
        public ICollection<Camion> CamionsConduits { get; set; } = new List<Camion>();
    }
}