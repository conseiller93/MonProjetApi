using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetApiNet.DTOs;
using ProjetApiNet.Services;

namespace ProjetApiNet.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StatistiquesController : ControllerBase
{
    private readonly IStatistiqueService _statistiqueService;

    public StatistiquesController(IStatistiqueService statistiqueService)
    {
        _statistiqueService = statistiqueService;
    }

    [HttpGet("journalieres")]
    public async Task<ActionResult<StatistiqueJournaliereDto>> GetJournalieres([FromQuery] DateTime? date)
    {
        var dateCible = date ?? DateTime.UtcNow;
        var stats = await _statistiqueService.GetStatistiquesJournalieresAsync(dateCible);
        return Ok(stats);
    }

    [HttpGet("mensuelles")]
    public async Task<ActionResult<StatistiqueMensuelleDto>> GetMensuelles([FromQuery] int? annee, [FromQuery] int? mois)
    {
        var anneeCible = annee ?? DateTime.UtcNow.Year;
        var moisCible = mois ?? DateTime.UtcNow.Month;

        if (moisCible < 1 || moisCible > 12)
            return Problem(detail: "Le mois doit être compris entre 1 et 12.", statusCode: 400);

        var stats = await _statistiqueService.GetStatistiquesMensuellesAsync(anneeCible, moisCible);
        return Ok(stats);
    }
}
