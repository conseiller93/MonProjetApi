using System;
using System.Collections.Generic;

namespace ProjetApiNet.DTOs;

public class StatistiqueGlobaleDto
{
    public int NombreChargements { get; set; }
    public decimal CarburantTotalLitres { get; set; }
    public decimal RevenuTotalGNF { get; set; }
    public decimal PrimesChauffeursGNF { get; set; }
    public decimal PrimesSuperviseursGroupeGNF { get; set; }
    public decimal PrimesSuperviseursZoneGNF { get; set; }
    public decimal PrimeSuperviseurGeneralGNF { get; set; }
}

public class StatistiqueParZoneDto
{
    public int ZoneMiniereId { get; set; }
    public required string ZoneNom { get; set; }
    public int NombreChargements { get; set; }
    public decimal CarburantTotalLitres { get; set; }
    public decimal RevenuTotalGNF { get; set; }
    public decimal PrimesVerseesGNF { get; set; }
}

public class StatistiqueParCamionDto
{
    public int CamionId { get; set; }
    public required string Immatriculation { get; set; }
    public required string ChauffeurIdentifiant { get; set; }
    public int NombreChargements { get; set; }
    public decimal CarburantTotalLitres { get; set; }
    public decimal PrimeChauffeurTotaleGNF { get; set; }

    // Renseigné uniquement pour les statistiques mensuelles
    public int? QuotaMensuelZone { get; set; }
    public bool QuotaDepasse { get; set; }
}

public class StatistiqueJournaliereDto
{
    public DateTime Date { get; set; }
    public StatistiqueGlobaleDto Global { get; set; } = new();
    public List<StatistiqueParZoneDto> ParZone { get; set; } = new();
    public List<StatistiqueParCamionDto> ParCamion { get; set; } = new();
}

public class StatistiqueMensuelleDto
{
    public int Annee { get; set; }
    public int Mois { get; set; }
    public StatistiqueGlobaleDto Global { get; set; } = new();
    public List<StatistiqueParZoneDto> ParZone { get; set; } = new();
    public List<StatistiqueParCamionDto> ParCamion { get; set; } = new();
}
