using System;
using System.ComponentModel.DataAnnotations;
using ProjetApiNet.Models;

namespace ProjetApiNet.DTOs;

public class ChargementDto
{
    public int Id { get; set; }
    public int CamionId { get; set; }
    public string? CamionImmatriculation { get; set; }
    public DateTime DateHeureDepart { get; set; }
    public DateTime? DateHeureRetour { get; set; }
    public StatutChargement Statut { get; set; }
    public decimal CarburantTotalLitres { get; set; }
    public decimal TarifGNF { get; set; }
}

public class ChargementCreateDto
{
    [Required]
    public int CamionId { get; set; }
    
    [Required]
    public DateTime DateHeureDepart { get; set; }
}

public class ChargementUpdateDto
{
    public DateTime? DateHeureRetour { get; set; }
    public StatutChargement? Statut { get; set; }
}

public class ChargementClotureDto
{
    [Required]
    public DateTime DateHeureRetour { get; set; }
}