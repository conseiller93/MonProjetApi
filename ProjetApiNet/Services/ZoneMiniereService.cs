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

    public static class ZoneMiniereConstants
    {
        public const int NombreMaxZones = 5;
        public const int LitresParKilometre = 2; // Règle métier : 2 litres par km[cite: 1]
        public static readonly TimeSpan HeureLimiteDepart = new TimeSpan(17, 30, 0); // Règle métier : Aucun départ après 17h30[cite: 1]

        // Injection des données strictes provenant du PDF Projet_TCA_Logistique_Miniere.pdf[cite: 1]
        public static readonly Dictionary<string, ZoneConfig> ConfigurationZones = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Bankoh"]   = new ZoneConfig(25, 4, 4500000, 104),   // 25km, 4 tours max, Tarification 4.500.000 GNF, Max 104 chrg/mois[cite: 1]
            ["Djoumaya"] = new ZoneConfig(37, 3, 5700000, 78),    // 37km, 3 tours max, Tarification 5.700.000 GNF, Max 78 chrg/mois[cite: 1]
            ["Kalima"]   = new ZoneConfig(22, 5, 4200000, 130),   // 22km, 5 tours max, Tarification 4.200.000 GNF, Max 130 chrg/mois[cite: 1]
            ["Timbi"]    = new ZoneConfig(55, 2, 7500000, 52),    // 55km, 2 tours max, Tarification 7.500.000 GNF, Max 52 chrg/mois[cite: 1]
            ["Soribaya"] = new ZoneConfig(35, 3, 5500000, 78)     // 35km, 3 tours max, Tarification 5.500.000 GNF, Max 78 chrg/mois[cite: 1]
        };
    }

    public record ZoneConfig(
        decimal DistanceDepotZone,
        int ToursMaxParJour,
        decimal Tarification,
        int ChargementsMaxParMois
    )
    {
        public decimal DistanceAllerRetour => DistanceDepotZone * 2;
    }

    public class ZoneMiniereService : IZoneMiniereService
    {
        private readonly IZoneMiniereRepository _zoneMiniereRepository;

        public ZoneMiniereService(IZoneMiniereRepository zoneMiniereRepository)
        {
            _zoneMiniereRepository = zoneMiniereRepository;
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

        public async Task<ZoneMiniereDto> CreateZoneAsync(ZoneMiniereCreateDto zoneMiniereCreateDto)
        {
            var zonesExistantes = await _zoneMiniereRepository.GetAllAsync();
            if (zonesExistantes.Count() >= ZoneMiniereConstants.NombreMaxZones)
            {
                throw new InvalidOperationException($"Impossible de créer une nouvelle zone : le projet TCA est strictement limité à {ZoneMiniereConstants.NombreMaxZones} zones minières.");
            }

            if (!ZoneMiniereConstants.ConfigurationZones.TryGetValue(zoneMiniereCreateDto.Nom, out var config))
            {
                throw new InvalidOperationException($"La zone '{zoneMiniereCreateDto.Nom}' n'est pas répertoriée dans le cahier des charges de TCA Guinée Mining SA.");
            }

            var zone = new ZoneMiniere
            {
                Nom = zoneMiniereCreateDto.Nom,
                DistanceDepotZone = config.DistanceDepotZone,
                DistanceAllerRetour = config.DistanceAllerRetour,
                ToursMaxParJour = config.ToursMaxParJour,
                Tarification = config.Tarification,
                ChargementsMaxParMois = config.ChargementsMaxParMois
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
                if (!ZoneMiniereConstants.ConfigurationZones.TryGetValue(zoneMiniereUpdateDto.Nom, out var config))
                {
                    throw new InvalidOperationException($"La zone '{zoneMiniereUpdateDto.Nom}' n'est pas valide.");
                }

                zoneExistante.Nom = zoneMiniereUpdateDto.Nom;
                zoneExistante.DistanceDepotZone = config.DistanceDepotZone;
                zoneExistante.DistanceAllerRetour = config.DistanceAllerRetour;
                zoneExistante.ToursMaxParJour = config.ToursMaxParJour;
                zoneExistante.Tarification = config.Tarification;
                zoneExistante.ChargementsMaxParMois = config.ChargementsMaxParMois;
            }

            _zoneMiniereRepository.Update(zoneExistante);
            return await _zoneMiniereRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteZoneAsync(int id)
        {
            var zone = await _zoneMiniereRepository.GetByIdAsync(id);
            if (zone == null) return false;

            _zoneMiniereRepository.Delete(zone);
            return await _zoneMiniereRepository.SaveChangesAsync();
        }
    }
}