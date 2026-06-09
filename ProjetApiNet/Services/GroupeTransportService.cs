using Mapster;
using TCA.API.DTOS;
using TCA.API.Models;
using TCA.API.Repositories;

namespace TCA.API.Services
{
    public interface IGroupeTransportService
    {
        Task<IEnumerable<GroupeTransportDto>> GetAllGroupesAsync();
        Task<GroupeTransportDto?> GetGroupeByIdAsync(int id);
        Task<GroupeTransportDto> CreateGroupeAsync(GroupeTransportCreateDto dto);
        Task<bool> UpdateGroupeAsync(int id, GroupeTransportUpdateDto dto);
        Task<bool> DeleteGroupeAsync(int id);
    }

    public class GroupeTransportService : IGroupeTransportService
    {
        private readonly IGroupeTransportRepository _groupeTransportRepository;
        private readonly IZoneMiniereRepository _zoneMiniereRepository;

        // Limites strictes définies dans la charte métier TCA
        private const int NombreMaxGroupesTotal = 10; 
        private const int NombreMaxGroupesParZone = 2;

        public GroupeTransportService(
            IGroupeTransportRepository groupeTransportRepository, 
            IZoneMiniereRepository zoneMiniereRepository)
        {
            _groupeTransportRepository = groupeTransportRepository;
            _zoneMiniereRepository = zoneMiniereRepository;
        }

        public async Task<IEnumerable<GroupeTransportDto>> GetAllGroupesAsync()
        {
            var groupes = await _groupeTransportRepository.GetAllAsync();
            return groupes.Select(g => new GroupeTransportDto
            {
                Id = g.Id,
                Nom = g.Nom,
                SuperviseurGroupeId = g.SuperviseurGroupeId,
                SuperviseurGroupeNom = g.SuperviseurGroupe != null ? $"{g.SuperviseurGroupe.Prenom} {g.SuperviseurGroupe.Nom}" : null,
                ZoneMiniereId = g.ZoneMiniereId,
                ZoneMiniereNom = g.ZoneMiniere?.Nom,
                NombreDeCamions = g.Camions?.Count ?? 0
            });
        }

        public async Task<GroupeTransportDto?> GetGroupeByIdAsync(int id)
        {
            var g = await _groupeTransportRepository.GetByIdAsync(id);
            if (g == null) return null;

            return new GroupeTransportDto
            {
                Id = g.Id,
                Nom = g.Nom,
                SuperviseurGroupeId = g.SuperviseurGroupeId,
                SuperviseurGroupeNom = g.SuperviseurGroupe != null ? $"{g.SuperviseurGroupe.Prenom} {g.SuperviseurGroupe.Nom}" : null,
                ZoneMiniereId = g.ZoneMiniereId,
                ZoneMiniereNom = g.ZoneMiniere?.Nom,
                NombreDeCamions = g.Camions?.Count ?? 0
            };
        }

        public async Task<GroupeTransportDto> CreateGroupeAsync(GroupeTransportCreateDto dto)
        {
            var tousLesGroupes = await _groupeTransportRepository.GetAllAsync();
            
            // Règle 1 : TCA dispose de 10 groupes au total
            if (tousLesGroupes.Count() >= NombreMaxGroupesTotal)
            {
                throw new InvalidOperationException($"Impossible de créer le groupe. La société TCA est limitée à un parc de {NombreMaxGroupesTotal} groupes de transport.");
            }

            var zone = await _zoneMiniereRepository.GetByIdAsync(dto.ZoneMiniereId);
            if (zone == null)
            {
                throw new KeyNotFoundException("La zone minière spécifiée n'existe pas.");
            }

            // Règle 2 : Répartition équitable (Ex: Bankoh -> Groupe 1 et 2, Djoumaya -> Groupe 3 et 4...) soit 2 groupes max par zone
            var groupesDansCetteZone = tousLesGroupes.Count(g => g.ZoneMiniereId == dto.ZoneMiniereId);
            if (groupesDansCetteZone >= NombreMaxGroupesParZone)
            {
                throw new InvalidOperationException($"La zone minière '{zone.Nom}' a déjà atteint son quota maximal de {NombreMaxGroupesParZone} groupes de transport.");
            }

            var groupe = dto.Adapt<GroupeTransport>();
            await _groupeTransportRepository.AddAsync(groupe);
            await _groupeTransportRepository.SaveChangesAsync();

            var resultat = await _groupeTransportRepository.GetByIdAsync(groupe.Id);
            return (resultat ?? groupe).Adapt<GroupeTransportDto>();
        }

        public async Task<bool> UpdateGroupeAsync(int id, GroupeTransportUpdateDto dto)
        {
            var groupeExistant = await _groupeTransportRepository.GetByIdAsync(id);
            if (groupeExistant == null) return false;

            if (dto.ZoneMiniereId.HasValue && dto.ZoneMiniereId.Value != groupeExistant.ZoneMiniereId)
            {
                var zoneCible = await _zoneMiniereRepository.GetByIdAsync(dto.ZoneMiniereId.Value);
                if (zoneCible == null)
                {
                    throw new KeyNotFoundException("La zone minière cible spécifiée n'existe pas.");
                }

                var tousLesGroupes = await _groupeTransportRepository.GetAllAsync();
                var groupesDansZoneCible = tousLesGroupes.Count(g => g.ZoneMiniereId == dto.ZoneMiniereId.Value && g.Id != id);

                if (groupesDansZoneCible >= NombreMaxGroupesParZone)
                {
                    throw new InvalidOperationException($"Mutation impossible. La zone cible '{zoneCible.Nom}' dispose déjà de {NombreMaxGroupesParZone} groupes.");
                }
            }

            dto.Adapt(groupeExistant);
            _groupeTransportRepository.Update(groupeExistant);
            return await _groupeTransportRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteGroupeAsync(int id)
        {
            var groupe = await _groupeTransportRepository.GetByIdAsync(id);
            if (groupe == null) return false;

            if (groupe.Camions != null && groupe.Camions.Any())
            {
                throw new InvalidOperationException("Impossible de supprimer un groupe de transport contenant encore des camions affectés.");
            }

            _groupeTransportRepository.Delete(groupe);
            return await _groupeTransportRepository.SaveChangesAsync();
        }
    }
}