using System;
using System.ComponentModel.DataAnnotations;
using ProjetApiNet.Models;

namespace ProjetApiNet.DTOs;

public class ChargementDto
{
    public int Id { get; set; }
    public int CamionId { get; set; }
    public string? CamionImmatriculation { get; set; }
    public int ZoneMiniereId { get; set; }
    public string? ZoneMiniereNom { get; set; }
    public DateTime DateHeureDepart { get; set; }
    public DateTime? DateHeureRetour { get; set; }
    public StatutChargement Statut { get; set; }
    public bool EstCloture { get; set; }

    // Données financières calculées automatiquement
    public decimal CarburantTotalLitres { get; set; }
    public decimal TarifGNF { get; set; }
    public decimal PrimeChauffeurGNF { get; set; }
    public decimal PrimeSuperviseurGroupeGNF { get; set; }
    public decimal PrimeSuperviseurZoneGNF { get; set; }
    public decimal PrimeSupervGenGNF { get; set; }
}

public class ChargementCreateDto
{
    [Required(ErrorMessage = "L'ID du camion est requis.")]
    public int CamionId { get; set; }

    // La zone minière est obligatoire pour calculer le carburant et les primes dès le départ
    [Required(ErrorMessage = "L'ID de la zone minière est requis.")]
    public int ZoneMiniereId { get; set; }

    [Required(ErrorMessage = "La date/heure de départ est requise.")]
    public DateTime DateHeureDepart { get; set; }
}

public class ChargementUpdateDto
{
    public DateTime? DateHeureRetour { get; set; }
    public StatutChargement? Statut { get; set; }
}

public class ChargementClotureDto
{
    [Required(ErrorMessage = "La date/heure de retour est requise pour clôturer un chargement.")]
    public DateTime DateHeureRetour { get; set; }
}