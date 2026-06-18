namespace FrontendMaui.Models;

public class ZoneMiniere
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public decimal DistanceDepotZone { get; set; }
    public decimal DistanceAllerRetour { get; set; }
    public int ToursMaxParJour { get; set; }
    public decimal Tarification { get; set; }
    public int ChargementsMaxParMois { get; set; }
}
