namespace ProjetApiNet.Models;

public class Chargement
{
    public int Id { get; set; }
    public int CamionId { get; set; }
    public Camion Camion { get; set; } = null!;

    public DateTime DateHeureDepart { get; set; }
    public DateTime? DateHeureRetour { get; set; }
    public StatutChargement Statut { get; set; } = StatutChargement.EnCours;

    // Conforme aux règles du carburant (2L / km d'un aller-retour complet)
    public decimal CarburantTotalLitres { get; set; }
    
    // Suivi financier imposé par le PDF (Rapports et Primes)
    public decimal TarifGNF { get; set; }
    public decimal PrimeChauffeurGNF { get; set; }
    public decimal PrimeSuperviseurGroupeGNF { get; set; }
    public decimal PrimeSuperviseurZoneGNF { get; set; }
    public decimal PrimeSupervGenGNF { get; set; }
}
