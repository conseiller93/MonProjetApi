namespace FrontendMaui.DTOs;

public class CamionDto
{
    public int Id { get; set; }
    public string Immatriculation { get; set; } = string.Empty;
    public string Modele { get; set; } = string.Empty;
    public int ChauffeurId { get; set; }
    public string? ChauffeurIdentifiant { get; set; }
    public int GroupeTransportId { get; set; }
    public string? GroupeTransportNom { get; set; }
    public decimal Kilometrage { get; set; }
}
