using System.ComponentModel.DataAnnotations;

namespace TCA.API.DTOS
{
    public class GroupeTransportDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = null!;
        public int SuperviseurGroupeId { get; set; }
        public string? SuperviseurGroupeNom { get; set; }
        public int ZoneMiniereId { get; set; }
        public string? ZoneMiniereNom { get; set; }
        public int NombreDeCamions { get; set; }
    }

    public class GroupeTransportCreateDto
    {
        [Required(ErrorMessage = "Le nom du groupe de transport est obligatoire.")]
        [MaxLength(150)]
        public string Nom { get; set; } = null!;

        [Required(ErrorMessage = "L'identifiant du superviseur de groupe est obligatoire.")]
        public int SuperviseurGroupeId { get; set; }

        [Required(ErrorMessage = "L'identifiant de la zone minière d'affectation est obligatoire.")]
        public int ZoneMiniereId { get; set; }
    }

    public class GroupeTransportUpdateDto
    {
        [MaxLength(150)]
        public string? Nom { get; set; }

        public int? SuperviseurGroupeId { get; set; }

        public int? ZoneMiniereId { get; set; }
    }
}
