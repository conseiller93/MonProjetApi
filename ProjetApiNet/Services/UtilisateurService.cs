using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ProjetApiNet.DTOs;
using ProjetApiNet.Models;
using ProjetApiNet.Repositories;

namespace ProjetApiNet.Services
{
    public interface IUtilisateurService
    {
        Task<IEnumerable<UtilisateurDto>> GetAllUtilisateursAsync();
        Task<UtilisateurDto?> GetUtilisateurByIdAsync(int id);
        Task<UtilisateurDto> InscrireUtilisateurAsync(UtilisateurCreateDto utilisateurCreateDto);
        Task<bool> UpdateUtilisateurAsync(int id, UtilisateurUpdateDto utilisateurUpdateDto);
        Task<bool> DeleteUtilisateurAsync(int id);
        Task<SessionResponseDto?> LoginAsync(ConnexionDto connexionDto);
    }

    public static class PersonnelConstants
    {
        // Quotas Max du PDF
        public const int NombreMaxChauffeurs = 100;
        public const int NombreMaxSuperviseurGroupe = 10;
        public const int NombreMaxSuperviseurZone = 5;
        public const int NombreMaxSuperviseurGeneral = 1;

        // Grille Salariale du PDF (GNF)
        public const decimal SalaireChauffeur = 4000000;
        public const decimal SalaireSuperviseurGroupe = 9000000;
        public const decimal SalaireSuperviseurZone = 14000000;
        public const decimal SalaireSuperviseurGeneral = 20000000;
    }

    public class UtilisateurService : IUtilisateurService
    {
        private readonly IUtilisateurRepository _utilisateurRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UtilisateurService> _logger;

        public UtilisateurService(
            IUtilisateurRepository utilisateurRepository,
            IConfiguration configuration,
            ILogger<UtilisateurService> logger)
        {
            _utilisateurRepository = utilisateurRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<UtilisateurDto>> GetAllUtilisateursAsync()
        {
            var utilisateurs = await _utilisateurRepository.GetAllAsync();
            return utilisateurs.Adapt<IEnumerable<UtilisateurDto>>();
        }

        public async Task<UtilisateurDto?> GetUtilisateurByIdAsync(int id)
        {
            var utilisateur = await _utilisateurRepository.GetByIdAsync(id);
            return utilisateur?.Adapt<UtilisateurDto>();
        }

        public async Task<UtilisateurDto> InscrireUtilisateurAsync(UtilisateurCreateDto utilisateurCreateDto)
        {
            var tousLesUtilisateurs = await _utilisateurRepository.GetAllAsync();

            if (tousLesUtilisateurs.Any(u => u.Identifiant.Equals(utilisateurCreateDto.Identifiant, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Tentative d'inscription échouée : l'identifiant {Identifiant} est déjà utilisé.", utilisateurCreateDto.Identifiant);
                throw new InvalidOperationException($"L'identifiant '{utilisateurCreateDto.Identifiant}' est déjà utilisé.");
            }

            // Vérification des quotas du PDF avant acceptation
            try
            {
                VerifierQuotaRole(tousLesUtilisateurs, utilisateurCreateDto.Role);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Tentative d'inscription de {Identifiant} refusée : {Message}", utilisateurCreateDto.Identifiant, ex.Message);
                throw;
            }

            var utilisateur = utilisateurCreateDto.Adapt<Utilisateur>();
            utilisateur.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(utilisateurCreateDto.MotDePasse);
            utilisateur.SalaireMensuelGNF = GetSalaireParRole(utilisateurCreateDto.Role);

            await _utilisateurRepository.AddAsync(utilisateur);
            await _utilisateurRepository.SaveChangesAsync();

            _logger.LogInformation("Utilisateur {Identifiant} inscrit avec succès avec le rôle {Role}.", utilisateur.Identifiant, utilisateur.Role);

            return utilisateur.Adapt<UtilisateurDto>();
        }

        public async Task<bool> UpdateUtilisateurAsync(int id, UtilisateurUpdateDto utilisateurUpdateDto)
        {
            var utilisateurExistant = await _utilisateurRepository.GetByIdAsync(id);
            if (utilisateurExistant == null) return false;

            var tousLesUtilisateurs = await _utilisateurRepository.GetAllAsync();

            // 1. Si le rôle change, on valide les quotas du nouveau rôle et on met à jour le salaire
            if (utilisateurUpdateDto.Role.HasValue && utilisateurUpdateDto.Role.Value != utilisateurExistant.Role)
            {
                VerifierQuotaRole(tousLesUtilisateurs, utilisateurUpdateDto.Role.Value, idExclu: id);
                utilisateurExistant.SalaireMensuelGNF = GetSalaireParRole(utilisateurUpdateDto.Role.Value);
            }

            // 2. Sauvegarder l'identifiant d'origine pour éviter que Mapster ne l'écrase avec du null
            var identifiantOrigine = utilisateurExistant.Identifiant;

            // 3. Appliquer le mapping automatique de Mapster sur le reste des champs
            utilisateurUpdateDto.Adapt(utilisateurExistant);

            // 4. Forcer la restauration de l'identifiant initial (ou appliquer le nouveau s'il est valide)
            if (!string.IsNullOrWhiteSpace(utilisateurUpdateDto.Identifiant))
            {
                if (!utilisateurUpdateDto.Identifiant.Equals(identifiantOrigine, StringComparison.OrdinalIgnoreCase))
                {
                    if (tousLesUtilisateurs.Any(u => u.Identifiant.Equals(utilisateurUpdateDto.Identifiant, StringComparison.OrdinalIgnoreCase) && u.Id != id))
                    {
                        throw new InvalidOperationException($"L'identifiant '{utilisateurUpdateDto.Identifiant}' est déjà utilisé.");
                    }
                    utilisateurExistant.Identifiant = utilisateurUpdateDto.Identifiant;
                }
            }
            else
            {
                // Si aucun identifiant n'est fourni dans le DTO, on garde précieusement celui de la base
                utilisateurExistant.Identifiant = identifiantOrigine;
            }

            // 5. Gestion du mot de passe
            if (!string.IsNullOrWhiteSpace(utilisateurUpdateDto.MotDePasse))
            {
                utilisateurExistant.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(utilisateurUpdateDto.MotDePasse);
            }

            _utilisateurRepository.Update(utilisateurExistant);
            return await _utilisateurRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteUtilisateurAsync(int id)
        {
            var utilisateur = await _utilisateurRepository.GetByIdAsync(id);
            if (utilisateur == null) return false;

            _utilisateurRepository.Delete(utilisateur);
            return await _utilisateurRepository.SaveChangesAsync();
        }

        public async Task<SessionResponseDto?> LoginAsync(ConnexionDto connexionDto)
        {
            var utilisateurs = await _utilisateurRepository.GetAllAsync();
            var utilisateur = utilisateurs.FirstOrDefault(u => u.Identifiant.Equals(connexionDto.Identifiant, StringComparison.OrdinalIgnoreCase));

            if (utilisateur == null || !BCrypt.Net.BCrypt.Verify(connexionDto.MotDePasse, utilisateur.MotDePasseHash))
            {
                _logger.LogWarning("Échec de connexion pour l'identifiant {Identifiant}.", connexionDto.Identifiant);
                return null;
            }

            var token = GenererTokenJWT(utilisateur);

            _logger.LogInformation("Connexion réussie pour l'utilisateur {Identifiant}.", utilisateur.Identifiant);

            return new SessionResponseDto
            {
                Utilisateur = utilisateur.Adapt<UtilisateurDto>(),
                Token = token
            };
        }

        private static void VerifierQuotaRole(IEnumerable<Utilisateur> utilisateurs, RoleUtilisateur role, int? idExclu = null)
        {
            var utilisateursFiltres = idExclu.HasValue
                ? utilisateurs.Where(u => u.Id != idExclu.Value)
                : utilisateurs;

            var compte = utilisateursFiltres.Count(u => u.Role == role);

            var (quota, libelle) = role switch
            {
                RoleUtilisateur.Chauffeur          => (PersonnelConstants.NombreMaxChauffeurs, "chauffeurs"),
                RoleUtilisateur.SuperviseurGroupe  => (PersonnelConstants.NombreMaxSuperviseurGroupe, "superviseurs de groupe"),
                RoleUtilisateur.SuperviseurZone    => (PersonnelConstants.NombreMaxSuperviseurZone, "superviseurs de zone"),
                RoleUtilisateur.SuperviseurGeneral => (PersonnelConstants.NombreMaxSuperviseurGeneral, "superviseur général"),
                _ => throw new InvalidOperationException("Rôle non spécifié ou inconnu.")
            };

            if (compte >= quota)
            {
                throw new InvalidOperationException($"Quota critique atteint pour le personnel : maximum {quota} {libelle} autorisé par la direction TCA.");
            }
        }

        private static decimal GetSalaireParRole(RoleUtilisateur role) => role switch
        {
            RoleUtilisateur.Chauffeur          => PersonnelConstants.SalaireChauffeur,
            RoleUtilisateur.SuperviseurGroupe  => PersonnelConstants.SalaireSuperviseurGroupe,
            RoleUtilisateur.SuperviseurZone    => PersonnelConstants.SalaireSuperviseurZone,
            RoleUtilisateur.SuperviseurGeneral => PersonnelConstants.SalaireSuperviseurGeneral,
            _ => 0
        };

        private string GenererTokenJWT(Utilisateur utilisateur)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "UneCleSuperSecreteDeMinimum32Caracteres!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
                new Claim(ClaimTypes.Name, utilisateur.Identifiant),
                new Claim(ClaimTypes.Role, utilisateur.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}