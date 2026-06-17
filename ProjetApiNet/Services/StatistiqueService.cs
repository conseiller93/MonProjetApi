using ProjetApiNet.DTOs;
using ProjetApiNet.Repositories;
using System.Linq;

namespace ProjetApiNet.Services;

public interface IStatistiqueService
{
    Task<StatistiqueJournaliereDto> GetStatistiquesJournalieresAsync(DateTime date);
    Task<StatistiqueMensuelleDto> GetStatistiquesMensuellesAsync(int annee, int mois);
}

public class StatistiqueService : IStatistiqueService
{
    private readonly IChargementRepository _chargementRepository;

    public StatistiqueService(IChargementRepository chargementRepository)
    {
        _chargementRepository = chargementRepository;
    }

    public async Task<StatistiqueJournaliereDto> GetStatistiquesJournalieresAsync(DateTime date)
    {
        var tousLesChargements = await _chargementRepository.GetAllAsync();

        var chargementsDuJour = tousLesChargements
            .Where(c => c.DateHeureDepart.Date == date.Date)
            .ToList();

        return new StatistiqueJournaliereDto
        {
            Date = date.Date,
            Global = ConstruireGlobal(chargementsDuJour),
            ParZone = ConstruireParZone(chargementsDuJour),
            ParCamion = ConstruireParCamion(chargementsDuJour, avecQuota: false)
        };
    }

    public async Task<StatistiqueMensuelleDto> GetStatistiquesMensuellesAsync(int annee, int mois)
    {
        var tousLesChargements = await _chargementRepository.GetAllAsync();

        var chargementsDuMois = tousLesChargements
            .Where(c => c.DateHeureDepart.Year == annee && c.DateHeureDepart.Month == mois)
            .ToList();

        return new StatistiqueMensuelleDto
        {
            Annee = annee,
            Mois = mois,
            Global = ConstruireGlobal(chargementsDuMois),
            ParZone = ConstruireParZone(chargementsDuMois),
            ParCamion = ConstruireParCamion(chargementsDuMois, avecQuota: true)
        };
    }

    // ----------------------------------------------------------
    // Construction des agrégats — uniquement sur chargements clôturés
    // (carburant et primes ne sont calculés qu'à la clôture, cf. ChargementService.CloturerChargementAsync)
    // ----------------------------------------------------------
    private static StatistiqueGlobaleDto ConstruireGlobal(List<Models.Chargement> chargements)
    {
        var clotures = chargements.Where(c => c.EstCloture).ToList();

        return new StatistiqueGlobaleDto
        {
            NombreChargements = chargements.Count,
            CarburantTotalLitres = clotures.Sum(c => c.CarburantTotalLitres),
            RevenuTotalGNF = chargements.Sum(c => c.TarifGNF),
            PrimesChauffeursGNF = clotures.Sum(c => c.PrimeChauffeurGNF),
            PrimesSuperviseursGroupeGNF = clotures.Sum(c => c.PrimeSuperviseurGroupeGNF),
            PrimesSuperviseursZoneGNF = clotures.Sum(c => c.PrimeSuperviseurZoneGNF),
            PrimeSuperviseurGeneralGNF = clotures.Sum(c => c.PrimeSupervGenGNF)
        };
    }

    private static List<StatistiqueParZoneDto> ConstruireParZone(List<Models.Chargement> chargements)
    {
        return chargements
            .GroupBy(c => new { c.ZoneMiniereId, c.ZoneMiniere.Nom })
            .Select(g => new StatistiqueParZoneDto
            {
                ZoneMiniereId = g.Key.ZoneMiniereId,
                ZoneNom = g.Key.Nom,
                NombreChargements = g.Count(),
                CarburantTotalLitres = g.Where(c => c.EstCloture).Sum(c => c.CarburantTotalLitres),
                RevenuTotalGNF = g.Sum(c => c.TarifGNF),
                PrimesVerseesGNF = g.Where(c => c.EstCloture)
                    .Sum(c => c.PrimeChauffeurGNF + c.PrimeSuperviseurGroupeGNF + c.PrimeSuperviseurZoneGNF)
            })
            .OrderBy(z => z.ZoneNom)
            .ToList();
    }

    private static List<StatistiqueParCamionDto> ConstruireParCamion(List<Models.Chargement> chargements, bool avecQuota)
    {
        return chargements
            .GroupBy(c => c.CamionId)
            .Select(g =>
            {
                var premier = g.First();
                var nombreChargements = g.Count();
                var quotaZone = avecQuota ? premier.ZoneMiniere.ChargementsMaxParMois : (int?)null;

                return new StatistiqueParCamionDto
                {
                    CamionId = g.Key,
                    Immatriculation = premier.Camion.Immatriculation,
                    ChauffeurIdentifiant = premier.Camion.Chauffeur?.Identifiant ?? "Inconnu",
                    NombreChargements = nombreChargements,
                    CarburantTotalLitres = g.Where(c => c.EstCloture).Sum(c => c.CarburantTotalLitres),
                    PrimeChauffeurTotaleGNF = g.Where(c => c.EstCloture).Sum(c => c.PrimeChauffeurGNF),
                    QuotaMensuelZone = quotaZone,
                    QuotaDepasse = avecQuota && quotaZone.HasValue && nombreChargements > quotaZone.Value
                };
            })
            .OrderByDescending(c => c.NombreChargements)
            .ToList();
    }
}
