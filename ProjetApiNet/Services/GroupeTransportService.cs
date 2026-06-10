using Mapster;
using ProjetApiNet.DTOs;        // CORRIGÉ : namespace unifié (plus de TCA.API.DTOS)
using ProjetApiNet.Models;      // CORRIGÉ : namespace unifié (plus de TCA.API.Models)
using ProjetApiNet.Repositories; // CORRIGÉ : namespace unifié (plus de TCA.API.Repositories)

namespace ProjetApiNet.Services  // CORRIGÉ : namespace unifié
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

        // Limites strictes définies dans le PDF TCA
        private const int NombreMaxGroupesTotal  = 10; // 10 groupes au total
        private const int NombreMaxGroupesParZone =  2; // 2 groupes par zone (ex: Bankoh → Gr.01 et Gr.02)

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

            // CORRIGÉ : Utilisateur n'a pas de propriétés Prenom/Nom, seulement Identifiant
            return groupes.Select(g => new GroupeTransportDto
            {
                Id                   = g.Id,
                Nom                  = g.Nom,
                SuperviseurGroupeId  = g.SuperviseurGroupeId,
                SuperviseurGroupeNom = g.SuperviseurGroupe?.Identifiant,
                ZoneMiniereId        = g.ZoneMiniereId,
                ZoneMiniereNom       = g.ZoneMiniere?.Nom,
                NombreDeCamions      = g.Camions?.Count ?? 0
            });
        }

        public async Task<GroupeTransportDto?> GetGroupeByIdAsync(int id)
        {
            var g = await _groupeTransportRepository.GetByIdAsync(id);
            if (g == null) return null;

            // CORRIGÉ : Identifiant au lieu de Prenom + Nom
            return new GroupeTransportDto
            {
                Id                   = g.Id,
                Nom                  = g.Nom,
                SuperviseurGroupeId  = g.SuperviseurGroupeId,
                SuperviseurGroupeNom = g.SuperviseurGroupe?.Identifiant,
                ZoneMiniereId        = g.ZoneMiniereId,
                ZoneMiniereNom       = g.ZoneMiniere?.Nom,
                NombreDeCamions      = g.Camions?.Count ?? 0
            };
        }

        // Créer un groupe — CONTRAINTES :
        //   • Max 10 groupes au total dans TCA
        //   • Max 2 groupes par zone minière
        //   • La zone cible doit exister
        public async Task<GroupeTransportDto> CreateGroupeAsync(GroupeTransportCreateDto dto)
        {
            var tousLesGroupes = await _groupeTransportRepository.GetAllAsync();

            // Limite globale TCA : 10 groupes maximum
            if (tousLesGroupes.Count() >= NombreMaxGroupesTotal)
            {
                throw new InvalidOperationException(
                    $"Impossible de créer le groupe : TCA est limité à {NombreMaxGroupesTotal} groupes de transport.");
            }

            // Vérification existence de la zone
            var zone = await _zoneMiniereRepository.GetByIdAsync(dto.ZoneMiniereId);
            if (zone == null)
            {
                throw new KeyNotFoundException(
                    $"La zone minière avec l'ID {dto.ZoneMiniereId} n'existe pas.");
            }

            // Limite par zone : 2 groupes max (Bankoh→01 et 02, Djoumaya→03 et 04, etc.)
            var groupesDansCetteZone = tousLesGroupes.Count(g => g.ZoneMiniereId == dto.ZoneMiniereId);
            if (groupesDansCetteZone >= NombreMaxGroupesParZone)
            {
                throw new InvalidOperationException(
                    $"La zone '{zone.Nom}' a déjà atteint son quota de {NombreMaxGroupesParZone} groupes de transport.");
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

            // Si la zone change, vérifier la capacité de la nouvelle zone
            if (dto.ZoneMiniereId.HasValue && dto.ZoneMiniereId.Value != groupeExistant.ZoneMiniereId)
            {
                var zoneCible = await _zoneMiniereRepository.GetByIdAsync(dto.ZoneMiniereId.Value);
                if (zoneCible == null)
                    throw new KeyNotFoundException($"La zone minière cible (ID: {dto.ZoneMiniereId.Value}) n'existe pas.");

                var tousLesGroupes = await _groupeTransportRepository.GetAllAsync();
                var groupesDansZoneCible = tousLesGroupes
                    .Count(g => g.ZoneMiniereId == dto.ZoneMiniereId.Value && g.Id != id);

                if (groupesDansZoneCible >= NombreMaxGroupesParZone)
                {
                    throw new InvalidOperationException(
                        $"Mutation impossible : la zone '{zoneCible.Nom}' a déjà {NombreMaxGroupesParZone} groupes.");
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

            // Sécurité : refuser la suppression si des camions sont encore affectés au groupe
            if (groupe.Camions != null && groupe.Camions.Any())
            {
                throw new InvalidOperationException(
                    "Impossible de supprimer un groupe qui contient encore des camions affectés.");
            }

            _groupeTransportRepository.Delete(groupe);
            return await _groupeTransportRepository.SaveChangesAsync();
        }
    }
}