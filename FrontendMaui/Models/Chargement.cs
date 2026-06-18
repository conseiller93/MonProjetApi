namespace FrontendMaui.Models;

public class Chargement
{
    public int Id { get; set; }
    public int CamionId { get; set; }
    public string? CamionImmatriculation { get; set; }
    public int ZoneMiniereId { get; set; }
    public string? ZoneMiniereNom { get; set; }
    public DateTime DateHeureDepart { get; set; }
    public DateTime? DateHeureRetour { get; set; }
    public string Statut { get; set; } = string.Empty;
    public bool EstCloture { get; set; }
    public decimal CarburantTotalLitres { get; set; }
    public decimal TarifGNF { get; set; }
    public decimal PrimeChauffeurGNF { get; set; }
    public decimal PrimeSuperviseurGroupeGNF { get; set; }
    public decimal PrimeSuperviseurZoneGNF { get; set; }
    public decimal PrimeSupervGenGNF { get; set; }
}
