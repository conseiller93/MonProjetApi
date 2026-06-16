using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetApiNet.DTOs;
using ProjetApiNet.Services;

namespace ProjetApiNet.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ZonesMinieresController : ControllerBase
    {
        private readonly IZoneMiniereService _zoneMiniereService;

        public ZonesMinieresController(IZoneMiniereService zoneMiniereService)
        {
            _zoneMiniereService = zoneMiniereService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ZoneMiniereDto>>> GetAll()
        {
            var zones = await _zoneMiniereService.GetAllZonesAsync();
            return Ok(zones);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ZoneMiniereDto>> GetById(int id)
        {
            var zone = await _zoneMiniereService.GetZoneByIdAsync(id);
            if (zone == null) return NotFound(new { Message = $"La zone minière {id} n'existe pas." });

            return Ok(zone);
        }

        [HttpPost]
        public async Task<ActionResult<ZoneMiniereDto>> Create(ZoneMiniereCreateDto dto)
        {
            try
            {
                var nouvelleZone = await _zoneMiniereService.CreateZoneAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = nouvelleZone.Id }, nouvelleZone);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: 400);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, ZoneMiniereUpdateDto dto)
        {
            try
            {
                var misAJour = await _zoneMiniereService.UpdateZoneAsync(id, dto);
                if (!misAJour) return NotFound(new { Message = $"Zone minière {id} introuvable." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: 400);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var supprime = await _zoneMiniereService.DeleteZoneAsync(id);
                if (!supprime) return NotFound(new { Message = $"Zone minière {id} introuvable." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: 400);
            }
        }
    }
}