using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjetApiNet.DTOs;
using ProjetApiNet.Services;

namespace ProjetApiNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChargementsController : ControllerBase
{
    private readonly IChargementService _chargementService;

    public ChargementsController(IChargementService chargementService)
    {
        _chargementService = chargementService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChargementDto>>> GetAll()
    {
        var chargements = await _chargementService.GetAllChargementsAsync();
        return Ok(chargements);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ChargementDto>> GetById(int id)
    {
        var chargement = await _chargementService.GetChargementByIdAsync(id);
        if (chargement == null) return NotFound($"Le chargement {id} n'existe pas.");

        return Ok(chargement);
    }

    [HttpPost]
    public async Task<ActionResult<ChargementDto>> Create(ChargementCreateDto dto)
    {
        try
        {
            var nouveauChargement = await _chargementService.CreateChargementAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = nouveauChargement.Id }, nouveauChargement);
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ChargementUpdateDto dto)
    {
        try
        {
            var misAJour = await _chargementService.UpdateChargementAsync(id, dto);
            if (!misAJour) return NotFound($"Chargement {id} introuvable.");

            return NoContent();
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}/cloturer")]
    public async Task<IActionResult> Cloturer(int id, ChargementClotureDto dto)
    {
        try
        {
            var cloture = await _chargementService.CloturerChargementAsync(id, dto);
            if (!cloture) return NotFound($"Impossible de clôturer : chargement {id} introuvable ou déjà clos.");

            return Ok("Le chargement a été clôturé avec succès.");
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            return NotFound("Configuration manquante.");
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var supprime = await _chargementService.DeleteChargementAsync(id);
        if (!supprime) return NotFound($"Chargement {id} introuvable.");

        return NoContent();
    }
}