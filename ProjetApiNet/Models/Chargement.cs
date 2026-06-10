namespace ProjetApiNet.Models;

public class Chargement
{
    public int Id { get; set; }

    // Camion qui effectue le chargement
    public int CamionId { get; set; }
    public Camion Camion { get; set; } = null!;

    // Zone minière destination (nécessaire pour les calculs carburant/primes sans naviguer via Camion→Groupe→Zone)
    public int ZoneMiniereId { get; set; }
    public ZoneMiniere ZoneMiniere { get; set; } = null!;

    // Horodatage départ et retour
    public DateTime DateHeureDepart { get; set; }
    public DateTime? DateHeureRetour { get; set; }

    // Statut : EnCours / Cloture
    public StatutChargement Statut { get; set; } = StatutChargement.EnCours;

    // Flag de clôture (redondant avec Statut mais pratique pour les requêtes LINQ)
    public bool EstCloture { get; set; } = false;

    // Carburant calculé automatiquement (2L/km × distance aller-retour)
    public decimal CarburantTotalLitres { get; set; }

    // Suivi financier (tarif et primes imposés par le PDF)
    public decimal TarifGNF { get; set; }
    public decimal PrimeChauffeurGNF { get; set; }
    public decimal PrimeSuperviseurGroupeGNF { get; set; }
    public decimal PrimeSuperviseurZoneGNF { get; set; }
    public decimal PrimeSupervGenGNF { get; set; }
}
