using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjetApiNet.DTOs;
using ProjetApiNet.Services;

namespace ProjetApiNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CamionsController : ControllerBase
    {
        private readonly ICamionService _camionService;

        public CamionsController(ICamionService camionService)
        {
            _camionService = camionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CamionDto>>> GetAll()
        {
            var camions = await _camionService.GetAllCamionsAsync();
            return Ok(camions);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CamionDto>> GetById(int id)
        {
            var camion = await _camionService.GetCamionByIdAsync(id);
            if (camion == null) 
                return NotFound($"Le camion avec l'ID {id} n'existe pas.");

            return Ok(camion);
        }

        [HttpPost]
        public async Task<ActionResult<CamionDto>> Create(CamionCreateDto dto)
        {
            try
            {
                var nouveauCamion = await _camionService.CreateCamionAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = nouveauCamion.Id }, nouveauCamion);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CamionUpdateDto dto)
        {
            try
            {
                var misAJour = await _camionService.UpdateCamionAsync(id, dto);
                if (!misAJour) 
                    return NotFound($"Camion {id} introuvable pour la mise à jour.");

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
            var supprime = await _camionService.DeleteCamionAsync(id);
            if (!supprime) 
                return NotFound($"Impossible de supprimer : camion {id} introuvable.");

            return NoContent();
        }
    }
}