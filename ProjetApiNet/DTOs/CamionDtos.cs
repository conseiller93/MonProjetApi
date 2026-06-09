using System.ComponentModel.DataAnnotations;

namespace ProjetApiNet.DTOs
{
    public class CamionDto
    {
        public int Id { get; set; }
        public required string Immatriculation { get; set; }
        public required string Modele { get; set; }
        public int ChauffeurId { get; set; }
        public string? ChauffeurIdentifiant { get; set; }
        public int GroupeTransportId { get; set; }
        public string? GroupeTransportNom { get; set; }
        public decimal Kilometrage { get; set; }
    }

    public class CamionCreateDto
    {
        [Required(ErrorMessage = "L'immatriculation est obligatoire.")]
        [MaxLength(20)]
        public required string Immatriculation { get; set; }

        [Required(ErrorMessage = "Le modèle du camion est obligatoire.")]
        [MaxLength(100)]
        public required string Modele { get; set; }

        [Required(ErrorMessage = "L'ID du chauffeur est requis.")]
        public int ChauffeurId { get; set; }

        [Required(ErrorMessage = "L'ID du groupe de transport est requis.")]
        public int GroupeTransportId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Le kilométrage doit être positif.")]
        public decimal Kilometrage { get; set; }
    }

    public class CamionUpdateDto
    {
        [MaxLength(20)]
        public string? Immatriculation { get; set; }

        [MaxLength(100)]
        public string? Modele { get; set; }

        public int? ChauffeurId { get; set; }

        public int? GroupeTransportId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Le kilométrage doit être positif.")]
        public decimal? Kilometrage { get; set; }
    }
}
