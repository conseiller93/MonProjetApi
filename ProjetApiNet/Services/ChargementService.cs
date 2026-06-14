using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.Extensions.Logging;
using ProjetApiNet.DTOs;
using ProjetApiNet.Models;
using ProjetApiNet.Repositories;

// CORRIGÉ : plus d'import "ProjetApiNet.Constants" (namespace inexistant)
// ZoneMiniereConstants est défini dans ZoneMiniereService.cs, même assembly, même namespace ProjetApiNet.Services

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
    private readonly IZoneMiniereRepository _zoneMiniereRepository;
    private readonly ILogger<ChargementService> _logger;

    public ChargementService(
        IChargementRepository chargementRepository,
        ICamionRepository camionRepository,
        IZoneMiniereRepository zoneMiniereRepository,
        ILogger<ChargementService> logger)
    {
        _chargementRepository = chargementRepository;
        _camionRepository = camionRepository;
        _zoneMiniereRepository = zoneMiniereRepository;
        _logger = logger;
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

    // Enregistrer un nouveau départ vers une zone minière
    // RÈGLES MÉTIER APPLIQUÉES :
    //   1. Aucun départ autorisé après 17h30
    //   2. Le camion doit avoir son dernier chargement clôturé (retour enregistré)
    //   3. Le nombre de tours de la journée ne doit pas dépasser le max de la zone
    //   4. Le tarif est fixé automatiquement selon la zone
    public async Task<ChargementDto> CreateChargementAsync(ChargementCreateDto dto)
    {
        // RÈGLE 1 — Aucun départ après 17h30
        if (dto.DateHeureDepart.TimeOfDay > ZoneMiniereConstants.HeureLimiteDepart)
        {
            _logger.LogWarning("Tentative d'enregistrement de départ après l'heure limite (17h30) pour le camion {CamionId}.", dto.CamionId);
            throw new InvalidOperationException(
                "Après 17h30 aucun nouveau départ ne doit être autorisé. " +
                "Les retours restent autorisés après cette heure.");
        }

        // Vérification existence du camion
        var camion = await _camionRepository.GetByIdAsync(dto.CamionId);
        if (camion == null)
        {
            _logger.LogWarning("Camion ID {CamionId} introuvable pour la création du trajet.", dto.CamionId);
            throw new InvalidOperationException($"Le camion avec l'ID {dto.CamionId} est introuvable.");
        }

        // Vérification existence de la zone et récupération de sa configuration
        var zone = await _zoneMiniereRepository.GetByIdAsync(dto.ZoneMiniereId);
        if (zone == null)
        {
            _logger.LogWarning("Zone ID {ZoneMiniereId} introuvable pour la création du trajet.", dto.ZoneMiniereId);
            throw new InvalidOperationException($"La zone minière avec l'ID {dto.ZoneMiniereId} est introuvable.");
        }

        // CORRIGÉ : accès direct à ZoneMiniereConstants.Zones (plus besoin de naviguer camion.Groupe.Zone)
        if (!ZoneMiniereConstants.Zones.TryGetValue(zone.Nom, out var zoneConfig))
        {
            _logger.LogWarning("Configuration manquante pour la zone '{ZoneNom}'.", zone.Nom);
            throw new InvalidOperationException($"Configuration introuvable pour la zone '{zone.Nom}'.");
        }

        // Récupération des chargements existants pour appliquer les règles 2 et 3
        var tousLesChargements = await _chargementRepository.GetAllAsync();

        // RÈGLE 2 — Un camion ne peut pas repartir sans retour enregistré
        var dernierChargement = tousLesChargements
            .Where(c => c.CamionId == dto.CamionId)
            .OrderByDescending(c => c.DateHeureDepart)
            .FirstOrDefault();

        if (dernierChargement != null && !dernierChargement.EstCloture)
        {
            _logger.LogWarning("Le camion {Immatriculation} (ID: {CamionId}) a déjà un trajet en cours non clôturé.", camion.Immatriculation, dto.CamionId);
            throw new InvalidOperationException(
                $"Le camion '{camion.Immatriculation}' est actuellement en zone. " +
                "Un retour doit être enregistré avant tout nouveau départ.");
        }

        // RÈGLE 3 — Nombre maximal de tours par jour selon la zone
        var toursDuJour = tousLesChargements
            .Count(c => c.CamionId == dto.CamionId &&
                        c.DateHeureDepart.Date == dto.DateHeureDepart.Date);

        if (toursDuJour >= zoneConfig.ToursMaxParJour)
        {
            _logger.LogWarning("Le nombre max de tours ({MaxTours}) est atteint pour le camion {CamionId} dans la zone {ZoneNom}.", zoneConfig.ToursMaxParJour, dto.CamionId, zone.Nom);
            throw new InvalidOperationException(
                $"Le nombre maximal de tours journaliers pour la zone '{zone.Nom}' est atteint " +
                $"({zoneConfig.ToursMaxParJour} tours/jour). Aucun nouveau départ possible aujourd'hui.");
        }

        // Construction du chargement avec application automatique du tarif
        var chargement = new Chargement
        {
            CamionId        = dto.CamionId,
            ZoneMiniereId   = dto.ZoneMiniereId,
            DateHeureDepart = dto.DateHeureDepart,
            Statut          = StatutChargement.EnCours,
            EstCloture      = false,
            TarifGNF        = zoneConfig.TarifChargementGNF
        };

        await _chargementRepository.AddAsync(chargement);
        await _chargementRepository.SaveChangesAsync();

        _logger.LogInformation("Trajet #{ChargementId} enregistré pour le camion {Immatriculation} vers la zone {ZoneNom} (Tarif : {Tarif} GNF).", chargement.Id, camion.Immatriculation, zone.Nom, chargement.TarifGNF);

        var chargementEnregistre = await _chargementRepository.GetByIdAsync(chargement.Id);
        return (chargementEnregistre ?? chargement).Adapt<ChargementDto>();
    }

    // Clôture d'un chargement (enregistrement du retour)
    // CALCULS AUTOMATIQUES :
    //   • Carburant total = distance aller-retour × 2 L/km
    //   • Primes chauffeur, superviseur groupe, superviseur zone, superviseur général
    // NOTE : Les retours sont autorisés même après 17h30 (règle PDF)
    public async Task<bool> CloturerChargementAsync(int id, ChargementClotureDto dto)
    {
        var chargement = await _chargementRepository.GetByIdAsync(id);
        if (chargement == null)
        {
            _logger.LogWarning("Tentative de clôture échouée : Trajet #{ChargementId} introuvable.", id);
            return false;
        }

        if (chargement.EstCloture)
        {
            _logger.LogWarning("Tentative de clôture échouée : Trajet #{ChargementId} déjà clôturé.", id);
            throw new InvalidOperationException($"Le chargement #{id} est déjà clôturé.");
        }

        // Récupération de la zone directement depuis le chargement (plus de navigation camion.Groupe.Zone)
        var zone = await _zoneMiniereRepository.GetByIdAsync(chargement.ZoneMiniereId);
        if (zone == null)
        {
            _logger.LogError("Incohérence de données : Zone minière ID {ZoneId} introuvable pour le trajet #{ChargementId}.", chargement.ZoneMiniereId, id);
            throw new InvalidOperationException($"Zone minière introuvable pour le chargement #{id}.");
        }

        if (!ZoneMiniereConstants.Zones.TryGetValue(zone.Nom, out var zoneConfig))
        {
            _logger.LogError("Configuration introuvable pour la zone '{ZoneNom}' liée au trajet #{ChargementId}.", zone.Nom, id);
            throw new InvalidOperationException($"Configuration introuvable pour la zone '{zone.Nom}'.");
        }

        chargement.DateHeureRetour = dto.DateHeureRetour;
        chargement.Statut          = StatutChargement.Cloture;
        chargement.EstCloture      = true;

        // Calcul automatique du carburant (2L/km × distance aller-retour)
        chargement.CarburantTotalLitres = zoneConfig.CarburantAllerRetourLitres;

        // Application des primes officielles (grilles PDF pages 3 et 4)
        chargement.PrimeChauffeurGNF         = zoneConfig.PrimeChauffeur;
        chargement.PrimeSuperviseurGroupeGNF = zoneConfig.PrimeSuperviseurGroupe;
        chargement.PrimeSuperviseurZoneGNF   = zoneConfig.PrimeSuperviseurZone;
        chargement.PrimeSupervGenGNF         = ZoneMiniereConstants.PrimeSupervGenParChargement;

        _chargementRepository.Update(chargement);
        var success = await _chargementRepository.SaveChangesAsync();

        if (success)
        {
            _logger.LogInformation("Trajet #{ChargementId} clôturé avec succès pour le camion ID {CamionId}. Carburant calculé : {Carburant}L. Prime chauffeur : {PrimeChauffeur} GNF.", id, chargement.CamionId, chargement.CarburantTotalLitres, chargement.PrimeChauffeurGNF);
        }
        else
        {
            _logger.LogError("Erreur lors de la sauvegarde de la clôture du trajet #{ChargementId} en base de données.", id);
        }

        return success;
    }

    public async Task<bool> UpdateChargementAsync(int id, ChargementUpdateDto dto)
    {
        var chargementExistant = await _chargementRepository.GetByIdAsync(id);
        if (chargementExistant == null) return false;

        if (chargementExistant.EstCloture)
            throw new InvalidOperationException("Un chargement clôturé ne peut plus être modifié.");

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