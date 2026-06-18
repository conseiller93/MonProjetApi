namespace FrontendMaui.Models;

public class GroupeTransport
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public int SuperviseurGroupeId { get; set; }
    public string? SuperviseurGroupeNom { get; set; }
    public int ZoneMiniereId { get; set; }
    public string? ZoneMiniereNom { get; set; }
    public int NombreDeCamions { get; set; }
}
