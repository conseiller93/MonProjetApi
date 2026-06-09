using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using ProjetApiNet.DTOs;
using ProjetApiNet.Models;
using ProjetApiNet.Repositories;

namespace ProjetApiNet.Services
{
    public interface ICamionService
    {
        Task<IEnumerable<CamionDto>> GetAllCamionsAsync();
        Task<CamionDto?> GetCamionByIdAsync(int id);
        Task<CamionDto> CreateCamionAsync(CamionCreateDto camionCreateDto);
        Task<bool> UpdateCamionAsync(int id, CamionUpdateDto camionUpdateDto);
        Task<bool> DeleteCamionAsync(int id);
    }

    public class CamionService : ICamionService
    {
        private readonly ICamionRepository _camionRepository;
        private readonly IGroupeTransportRepository _groupeTransportRepository;

        // Limites strictes définies dans le PDF
        private const int NombreMaxCamionsFlotte = 100;
        private const int NombreMaxCamionsParGroupe = 10;

        public CamionService(ICamionRepository camionRepository, IGroupeTransportRepository groupeTransportRepository)
        {
            _camionRepository = camionRepository;
            _groupeTransportRepository = groupeTransportRepository;
        }

        public async Task<IEnumerable<CamionDto>> GetAllCamionsAsync()
        {
            var camions = await _camionRepository.GetAllAsync();
            return camions.Adapt<IEnumerable<CamionDto>>();
        }

        public async Task<CamionDto?> GetCamionByIdAsync(int id)
        {
            var camion = await _camionRepository.GetByIdAsync(id);
            return camion?.Adapt<CamionDto>();
        }

        public async Task<CamionDto> CreateCamionAsync(CamionCreateDto camionCreateDto)
        {
            var tousCamions = await _camionRepository.GetAllAsync();

            // 1. Limite globale de la flotte TCA (100 camions max)
            if (tousCamions.Count() >= NombreMaxCamionsFlotte)
            {
                throw new InvalidOperationException($"Limite de flotte atteinte. L'entreprise TCA ne peut pas posséder plus de {NombreMaxCamionsFlotte} camions.");
            }

            // 2. Unicité de l'immatriculation
            if (tousCamions.Any(c => c.Immatriculation.Equals(camionCreateDto.Immatriculation, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Un camion avec l'immatriculation '{camionCreateDto.Immatriculation}' existe déjà.");
            }

            // 3. Vérification de la capacité du groupe (10 camions max par groupe)
            var groupe = await _groupeTransportRepository.GetByIdAsync(camionCreateDto.GroupeTransportId);
            if (groupe == null)
            {
                throw new InvalidOperationException($"Le groupe de transport sélectionné (ID: {camionCreateDto.GroupeTransportId}) n'existe pas.");
            }

            int camionsDansCeGroupe = tousCamions.Count(c => c.GroupeTransportId == camionCreateDto.GroupeTransportId);
            if (camionsDansCeGroupe >= NombreMaxCamionsParGroupe)
            {
                throw new InvalidOperationException($"Le groupe '{groupe.Nom}' a déjà atteint son quota maximal de {NombreMaxCamionsParGroupe} camions.");
            }

            // 4. Règle exclusive : Un chauffeur ne peut conduire qu'un seul camion à la fois
            var camionDuChauffeur = tousCamions.FirstOrDefault(c => c.ChauffeurId == camionCreateDto.ChauffeurId);
            if (camionDuChauffeur != null)
            {
                throw new InvalidOperationException($"Ce chauffeur conduit déjà le camion immatriculé '{camionDuChauffeur.Immatriculation}'. Un chauffeur ne peut être affecté qu'à un seul véhicule.");
            }

            var camion = camionCreateDto.Adapt<Camion>();

            await _camionRepository.AddAsync(camion);
            await _camionRepository.SaveChangesAsync();

            // Re-récupérer avec les inclusions pour avoir les libellés de retour dans le DTO
            var nouveauCamion = await _camionRepository.GetByIdAsync(camion.Id);
            return nouveauCamion!.Adapt<CamionDto>();
        }

        public async Task<bool> UpdateCamionAsync(int id, CamionUpdateDto camionUpdateDto)
        {
            var camionExistant = await _camionRepository.GetByIdAsync(id);
            if (camionExistant == null) return false;

            var tousCamions = await _camionRepository.GetAllAsync();

            // Validation de l'immatriculation si modifiée
            if (!string.IsNullOrWhiteSpace(camionUpdateDto.Immatriculation) && 
                !camionUpdateDto.Immatriculation.Equals(camionExistant.Immatriculation, StringComparison.OrdinalIgnoreCase))
            {
                if (tousCamions.Any(c => c.Immatriculation.Equals(camionUpdateDto.Immatriculation, StringComparison.OrdinalIgnoreCase) && c.Id != id))
                {
                    throw new InvalidOperationException($"L'immatriculation '{camionUpdateDto.Immatriculation}' est déjà attribuée à un autre véhicule.");
                }
            }

            // Validation du quota du groupe si changement de groupe
            if (camionUpdateDto.GroupeTransportId.HasValue && camionUpdateDto.GroupeTransportId.Value != camionExistant.GroupeTransportId)
            {
                var nouveauGroupe = await _groupeTransportRepository.GetByIdAsync(camionUpdateDto.GroupeTransportId.Value);
                if (nouveauGroupe == null)
                {
                    throw new InvalidOperationException("Le groupe de transport cible n'existe pas.");
                }

                int camionsDansNouveauGroupe = tousCamions.Count(c => c.GroupeTransportId == camionUpdateDto.GroupeTransportId.Value && c.Id != id);
                if (camionsDansNouveauGroupe >= NombreMaxCamionsParGroupe)
                {
                    throw new InvalidOperationException($"Le groupe cible '{nouveauGroupe.Nom}' est complet ({NombreMaxCamionsParGroupe} camions maximum).");
                }
            }

            // Validation de l'exclusivité du chauffeur si changement
            if (camionUpdateDto.ChauffeurId.HasValue && camionUpdateDto.ChauffeurId.Value != camionExistant.ChauffeurId)
            {
                var camionDuChauffeur = tousCamions.FirstOrDefault(c => c.ChauffeurId == camionUpdateDto.ChauffeurId.Value && c.Id != id);
                if (camionDuChauffeur != null)
                {
                    throw new InvalidOperationException($"Ce chauffeur est déjà assigné au camion '{camionDuChauffeur.Immatriculation}'.");
                }
            }

            camionUpdateDto.Adapt(camionExistant);
            _camionRepository.Update(camionExistant);
            return await _camionRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteCamionAsync(int id)
        {
            var camion = await _camionRepository.GetByIdAsync(id);
            if (camion == null) return false;

            _camionRepository.Delete(camion);
            return await _camionRepository.SaveChangesAsync();
        }
    }
}