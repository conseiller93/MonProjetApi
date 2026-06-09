using System.ComponentModel.DataAnnotations;
using ProjetApiNet.Models;

namespace ProjetApiNet.DTOs
{
    public class UtilisateurDto
    {
        public int Id { get; set; }
        public required string Identifiant { get; set; }
        public RoleUtilisateur Role { get; set; }
        public decimal SalaireMensuelGNF { get; set; }
    }

    public class UtilisateurCreateDto
    {
        [Required(ErrorMessage = "L'identifiant est obligatoire.")]
        [MaxLength(100)]
        public required string Identifiant { get; set; }

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        public required string MotDePasse { get; set; }

        [Required]
        public RoleUtilisateur Role { get; set; }
    }

    public class UtilisateurUpdateDto
    {
        [MaxLength(100)]
        public string? Identifiant { get; set; }

        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        public string? MotDePasse { get; set; }

        public RoleUtilisateur? Role { get; set; }
    }

    public class ConnexionDto
    {
        [Required(ErrorMessage = "L'identifiant est requis.")]
        public required string Identifiant { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        public required string MotDePasse { get; set; }
    }

    public class SessionResponseDto
    {
        public required UtilisateurDto Utilisateur { get; set; }
        public required string Token { get; set; }
    }
}
