using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using ProjetApiNet.Constants;
using ProjetApiNet.DTOs;
using ProjetApiNet.Models;
using ProjetApiNet.Repositories;

namespace ProjetApiNet.Services;

public interface IChargementService
{
    Task<IEnumerable<ChargementDto>> GetAllChargementsAsync();
    Task<ChargementDto?> GetChargementByIdAsync(int id);
    Task<ChargementDto> CreateChargementAsync(ChargementCreateDto dto);
    Task<bool> UpdateChargementAsync(int id, ChargementUpdateDto dto);
    Task<bool> CloturerChargementAsync(int id, ChargementClotureDto dto);
    Task<bool> DeleteChargementAsync(int id);
}

public class ChargementService : IChargementService
{
    private readonly IChargementRepository _chargementRepository;
    private readonly ICamionRepository _camionRepository;

    public ChargementService(IChargementRepository chargementRepository, ICamionRepository camionRepository)
    {
        _chargementRepository = chargementRepository;
        _camionRepository = camionRepository;
    }

    public async Task<IEnumerable<ChargementDto>> GetAllChargementsAsync()
    {
        var chargements = await _chargementRepository.GetAllAsync();
        return chargements.Adapt<IEnumerable<ChargementDto>>();
    }

    public async Task<ChargementDto?> GetChargementByIdAsync(int id)
    {
        var chargement = await _chargementRepository.GetByIdAsync(id);
        return chargement?.Adapt<ChargementDto>();
    }

    public async Task<ChargementDto> CreateChargementAsync(ChargementCreateDto dto)
    {
        // Règle 1 : Pas de départ après 17h30
        if (dto.DateHeureDepart.TimeOfDay > ZoneMiniereConstants.HeureLimiteDepart)
        {
            throw new InvalidOperationException("Après 17h30 aucun nouveau départ ne doit être autorisé.");
        }

        var camion = await _camionRepository.GetByIdAsync(dto.CamionId);
        if (camion == null) throw new InvalidOperationException("Camion introuvable.");
        
        if (camion.Groupe?.Zone == null || !ZoneMiniereConstants.Zones.TryGetValue(camion.Groupe.Zone.Nom, out var zoneConfig))
        {
            throw new InvalidOperationException("Configuration de la zone de ce camion introuvable.");
        }

        // Règle 2 : Un camion ne peut pas repartir sans retour enregistré
        var tousLesChargements = await _chargementRepository.GetAllAsync();
        var dernierChargement = tousLesChargements
            .Where(c => c.CamionId == dto.CamionId)
            .OrderByDescending(c => c.DateHeureDepart)
            .FirstOrDefault();

        if (dernierChargement != null && dernierChargement.Statut == StatutChargement.EnCours)
        {
            throw new InvalidOperationException("Un camion ne peut pas repartir sans retour enregistré.");
        }

        // Règle 3 : Nombre maximal de tours par jour non dépassé
        var aujourdhui = dto.DateHeureDepart.Date;
        var toursDuJour = tousLesChargements
            .Count(c => c.CamionId == dto.CamionId && c.DateHeureDepart.Date == aujourdhui);

        if (toursDuJour >= zoneConfig.ToursMaxParJour)
        {
            throw new InvalidOperationException($"Nombre maximal de chargements par jour atteint ({zoneConfig.ToursMaxParJour}).");
        }

        var chargement = new Chargement
        {
            CamionId = dto.CamionId,
            DateHeureDepart = dto.DateHeureDepart,
            Statut = StatutChargement.EnCours,
            TarifGNF = zoneConfig.TarifChargementGNF
        };

        await _chargementRepository.AddAsync(chargement);
        await _chargementRepository.SaveChangesAsync();

        return chargement.Adapt<ChargementDto>();
    }

    public async Task<bool> CloturerChargementAsync(int id, ChargementClotureDto dto)
    {
        var chargement = await _chargementRepository.GetByIdAsync(id);
        if (chargement == null || chargement.Statut == StatutChargement.Cloture) return false;

        var camion = await _camionRepository.GetByIdAsync(chargement.CamionId);
        if (camion?.Groupe?.Zone == null || !ZoneMiniereConstants.Zones.TryGetValue(camion.Groupe.Zone.Nom, out var zoneConfig))
        {
            throw new InvalidOperationException("Données de la zone introuvables pour les calculs.");
        }

        chargement.DateHeureRetour = dto.DateHeureRetour;
        chargement.Statut = StatutChargement.Cloture;

        // Calcul automatique du carburant (2 litres par kilomètre d'aller-retour)
        chargement.CarburantTotalLitres = zoneConfig.DistanceAllerRetourKm * ZoneMiniereConstants.LitresParKilometre;

        // Association des primes d'après les grilles officielles
        chargement.PrimeChauffeurGNF = zoneConfig.PrimeChauffeur;
        chargement.PrimeSuperviseurGroupeGNF = zoneConfig.PrimeSuperviseurGroupe;
        chargement.PrimeSuperviseurZoneGNF = zoneConfig.PrimeSuperviseurZone;
        chargement.PrimeSupervGenGNF = ZoneMiniereConstants.PrimeSupervGenParChargement;

        _chargementRepository.Update(chargement);
        return await _chargementRepository.SaveChangesAsync();
    }

    public async Task<bool> UpdateChargementAsync(int id, ChargementUpdateDto dto)
    {
        var chargementExistant = await _chargementRepository.GetByIdAsync(id);
        if (chargementExistant == null) return false;

        if (chargementExistant.Statut == StatutChargement.Cloture)
        {
            throw new InvalidOperationException("Un chargement clôturé ne peut plus être modifié.");
        }

        dto.Adapt(chargementExistant);
        _chargementRepository.Update(chargementExistant);
        return await _chargementRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteChargementAsync(int id)
    {
        var chargement = await _chargementRepository.GetByIdAsync(id);
        if (chargement == null) return false;

        _chargementRepository.Delete(chargement);
        return await _chargementRepository.SaveChangesAsync();
    }
}