using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjetApiNet.DTOs;
using ProjetApiNet.Services;

namespace ProjetApiNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilisateursController : ControllerBase
    {
        private readonly IUtilisateurService _utilisateurService;

        public UtilisateursController(IUtilisateurService utilisateurService)
        {
            _utilisateurService = utilisateurService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UtilisateurDto>>> GetAll()
        {
            var utilisateurs = await _utilisateurService.GetAllUtilisateursAsync();
            return Ok(utilisateurs);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UtilisateurDto>> GetById(int id)
        {
            var utilisateur = await _utilisateurService.GetUtilisateurByIdAsync(id);
            if (utilisateur == null) 
                return NotFound($"L'utilisateur avec l'ID {id} n'existe pas.");
            
            return Ok(utilisateur);
        }

        [HttpPost("inscrire")]
        public async Task<ActionResult<UtilisateurDto>> Inscrire(UtilisateurCreateDto dto)
        {
            try
            {
                var nouvelUtilisateur = await _utilisateurService.InscrireUtilisateurAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = nouvelUtilisateur.Id }, nouvelUtilisateur);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<SessionResponseDto>> Login(ConnexionDto dto)
        {
            var session = await _utilisateurService.LoginAsync(dto);
            if (session == null) 
                return Unauthorized("Identifiant ou mot de passe incorrect.");

            return Ok(session);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UtilisateurUpdateDto dto)
        {
            try
            {
                var misAJour = await _utilisateurService.UpdateUtilisateurAsync(id, dto);
                if (!misAJour) 
                    return NotFound($"Impossible de mettre à jour : utilisateur {id} introuvable.");

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var supprime = await _utilisateurService.DeleteUtilisateurAsync(id);
            if (!supprime) 
                return NotFound($"Impossible de supprimer : utilisateur {id} introuvable.");

            return NoContent();
        }
    }
}