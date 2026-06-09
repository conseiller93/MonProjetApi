using Microsoft.AspNetCore.Mvc;
using TCA.API.DTOS;
using TCA.API.Services;

namespace TCA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Optionnel : À activer avec votre infrastructure JWT Authentication
    public class GroupesTransportController : ControllerBase
    {
        private readonly IGroupeTransportService _groupeTransportService;

        public GroupesTransportController(IGroupeTransportService groupeTransportService)
        {
            _groupeTransportService = groupeTransportService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupeTransportDto>>> GetAll()
        {
            var groupes = await _groupeTransportService.GetAllGroupesAsync();
            return Ok(groupes);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GroupeTransportDto>> GetById(int id)
        {
            var groupe = await _groupeTransportService.GetGroupeByIdAsync(id);
            if (groupe == null) 
            {
                return NotFound(new { Message = $"Le groupe de transport avec l'ID {id} n'existe pas." });
            }

            return Ok(groupe);
        }

        [HttpPost]
        public async Task<ActionResult<GroupeTransportDto>> Create([FromBody] GroupeTransportCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var nouveauGroupe = await _groupeTransportService.CreateGroupeAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = nouveauGroupe.Id }, nouveauGroupe);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] GroupeTransportUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var misAJour = await _groupeTransportService.UpdateGroupeAsync(id, dto);
                if (!misAJour) 
                {
                    return NotFound(new { Message = $"Groupe de transport avec l'ID {id} introuvable." });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var supprime = await _groupeTransportService.DeleteGroupeAsync(id);
                if (!supprime) 
                {
                    return NotFound(new { Message = $"Groupe de transport avec l'ID {id} introuvable." });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}