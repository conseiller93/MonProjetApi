using Mapster;
using ProjetApiNet.DTOs;
using ProjetApiNet.Models;
using ProjetApiNet.Repositories;

namespace ProjetApiNet.Services
{
    public interface IZoneMiniereService
    {
        Task<IEnumerable<ZoneMiniereDto>> GetAllZonesAsync();
        Task<ZoneMiniereDto?> GetZoneByIdAsync(int id);
        Task<ZoneMiniereDto> CreateZoneAsync(ZoneMiniereCreateDto zoneMiniereCreateDto);
        Task<bool> UpdateZoneAsync(int id, ZoneMiniereUpdateDto zoneMiniereUpdateDto);
        Task<bool> DeleteZoneAsync(int id);
    }

    // ============================================================
    // CONSTANTES MÉTIER — Source : Projet_TCA_Logistique_Miniere.pdf
    // ============================================================
    public static class ZoneMiniereConstants
    {
        // Limite stricte : TCA dispose de 5 zones minières
        public const int NombreMaxZones = 5;

        // Règle carburant : 2 litres consommés par kilomètre parcouru
        public const decimal LitresParKilometre = 2m;

        // Règle horaire : aucun nouveau départ après 17h30 (les retours restent autorisés)
        public static readonly TimeSpan HeureLimiteDepart = new TimeSpan(17, 30, 0);

        // Prime fixe du superviseur général : 2 300 GNF par chargement effectué
        public const decimal PrimeSupervGenParChargement = 2_300m;

        // Données officielles des 5 zones (PDF pages 2, 3 et 4)
        // Paramètres : DistanceDepotKm, ToursMaxParJour, TarifChargementGNF,
        //              ChargementsMaxParMoisParCamion,
        //              PrimeChauffeur, PrimeSuperviseurGroupe, PrimeSuperviseurZone
        public static readonly Dictionary<string, ZoneConfig> Zones =
            new(StringComparer.OrdinalIgnoreCase)
        {
            ["Bankoh"]   = new ZoneConfig(25, 4, 4_500_000m, 104,  43_000m,  9_000m,  7_000m),
            ["Djoumaya"] = new ZoneConfig(37, 3, 5_700_000m,  78,  58_000m, 12_000m,  9_500m),
            ["Kalima"]   = new ZoneConfig(22, 5, 4_200_000m, 130,  35_000m,  7_500m,  5_500m),
            ["Timbi"]    = new ZoneConfig(55, 2, 7_500_000m,  52,  88_000m, 18_000m, 14_000m),
            ["Soribaya"] = new ZoneConfig(35, 3, 5_500_000m,  78,  58_000m, 12_000m,  9_500m),
        };
    }

    // Objet de configuration immuable pour chaque zone
    public record ZoneConfig(
        int    DistanceDepotKm,
        int    ToursMaxParJour,
        decimal TarifChargementGNF,
        int    ChargementsMaxParMoisParCamion,
        decimal PrimeChauffeur,
        decimal PrimeSuperviseurGroupe,
        decimal PrimeSuperviseurZone
    )
    {
        // Distance aller-retour calculée automatiquement
        public int     DistanceAllerRetourKm    => DistanceDepotKm * 2;

        // Carburant consommé pour un aller-retour complet (en litres)
        public decimal CarburantAllerRetourLitres =>
            DistanceAllerRetourKm * ZoneMiniereConstants.LitresParKilometre;
    }

    // ============================================================
    // SERVICE
    // ============================================================
    public class ZoneMiniereService : IZoneMiniereService
    {
        private readonly IZoneMiniereRepository _zoneMiniereRepository;
        private readonly IChargementRepository _chargementRepository;
        private readonly IGroupeTransportRepository _groupeTransportRepository;

        public ZoneMiniereService(
            IZoneMiniereRepository zoneMiniereRepository,
            IChargementRepository chargementRepository,
            IGroupeTransportRepository groupeTransportRepository)
        {
            _zoneMiniereRepository = zoneMiniereRepository;
            _chargementRepository = chargementRepository;
            _groupeTransportRepository = groupeTransportRepository;
        }

        public async Task<IEnumerable<ZoneMiniereDto>> GetAllZonesAsync()
        {
            var zones = await _zoneMiniereRepository.GetAllAsync();
            return zones.Adapt<IEnumerable<ZoneMiniereDto>>();
        }

        public async Task<ZoneMiniereDto?> GetZoneByIdAsync(int id)
        {
            var zone = await _zoneMiniereRepository.GetByIdAsync(id);
            return zone?.Adapt<ZoneMiniereDto>();
        }

        // Créer une zone — CONTRAINTES :
        //   • Max 5 zones dans le système TCA
        //   • Le nom doit correspondre exactement à une des 5 zones officielles
        //   • Toutes les données (distance, tarif, tours max) sont appliquées automatiquement
        public async Task<ZoneMiniereDto> CreateZoneAsync(ZoneMiniereCreateDto zoneMiniereCreateDto)
        {
            var zonesExistantes = await _zoneMiniereRepository.GetAllAsync();

            if (zonesExistantes.Count() >= ZoneMiniereConstants.NombreMaxZones)
            {
                throw new InvalidOperationException(
                    $"Impossible de créer une nouvelle zone : TCA est limité à " +
                    $"{ZoneMiniereConstants.NombreMaxZones} zones minières " +
                    $"(Bankoh, Djoumaya, Kalima, Timbi, Soribaya).");
            }

            if (!ZoneMiniereConstants.Zones.TryGetValue(zoneMiniereCreateDto.Nom, out var config))
            {
                throw new InvalidOperationException(
                    $"La zone '{zoneMiniereCreateDto.Nom}' n'est pas répertoriée dans le cahier des charges TCA. " +
                    $"Zones autorisées : {string.Join(", ", ZoneMiniereConstants.Zones.Keys)}.");
            }

            // Vérifier qu'une zone de même nom n'existe pas déjà
            if (zonesExistantes.Any(z => z.Nom.Equals(zoneMiniereCreateDto.Nom, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"La zone '{zoneMiniereCreateDto.Nom}' existe déjà.");
            }

            // Application automatique des données officielles du PDF
            var zone = new ZoneMiniere
            {
                Nom                   = zoneMiniereCreateDto.Nom,
                DistanceDepotZone     = config.DistanceDepotKm,
                DistanceAllerRetour   = config.DistanceAllerRetourKm,
                ToursMaxParJour       = config.ToursMaxParJour,
                Tarification          = config.TarifChargementGNF,
                ChargementsMaxParMois = config.ChargementsMaxParMoisParCamion
            };

            await _zoneMiniereRepository.AddAsync(zone);
            await _zoneMiniereRepository.SaveChangesAsync();

            return zone.Adapt<ZoneMiniereDto>();
        }

        public async Task<bool> UpdateZoneAsync(int id, ZoneMiniereUpdateDto zoneMiniereUpdateDto)
        {
            var zoneExistante = await _zoneMiniereRepository.GetByIdAsync(id);
            if (zoneExistante == null) return false;

            if (!string.IsNullOrWhiteSpace(zoneMiniereUpdateDto.Nom))
            {
                if (!ZoneMiniereConstants.Zones.TryGetValue(zoneMiniereUpdateDto.Nom, out var config))
                {
                    throw new InvalidOperationException(
                        $"La zone '{zoneMiniereUpdateDto.Nom}' n'est pas valide. " +
                        $"Zones autorisées : {string.Join(", ", ZoneMiniereConstants.Zones.Keys)}.");
                }

                // Resynchronisation avec les données officielles
                zoneExistante.Nom                   = zoneMiniereUpdateDto.Nom;
                zoneExistante.DistanceDepotZone      = config.DistanceDepotKm;
                zoneExistante.DistanceAllerRetour    = config.DistanceAllerRetourKm;
                zoneExistante.ToursMaxParJour        = config.ToursMaxParJour;
                zoneExistante.Tarification           = config.TarifChargementGNF;
                zoneExistante.ChargementsMaxParMois  = config.ChargementsMaxParMoisParCamion;
            }

            _zoneMiniereRepository.Update(zoneExistante);
            return await _zoneMiniereRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteZoneAsync(int id)
        {
            var zone = await _zoneMiniereRepository.GetByIdAsync(id);
            if (zone == null) return false;

            // 1. Vérifier s'il y a des chargements liés (actifs ou passés)
            var tousLesChargements = await _chargementRepository.GetAllAsync();
            bool aDesChargements = tousLesChargements.Any(c => c.ZoneMiniereId == id);
            if (aDesChargements)
            {
                throw new InvalidOperationException("Impossible de supprimer cette zone car elle est liée à des chargements actifs ou passés.");
            }

            // 2. Vérifier s'il y a des Groupes de Transport liés
            var tousLesGroupes = await _groupeTransportRepository.GetAllAsync();
            bool aDesGroupes = tousLesGroupes.Any(g => g.ZoneMiniereId == id);
            if (aDesGroupes)
            {
                throw new InvalidOperationException("Impossible de supprimer cette zone car un groupe de transport y est affecté.");
            }

            _zoneMiniereRepository.Delete(zone);
            return await _zoneMiniereRepository.SaveChangesAsync();
        }
    }
}