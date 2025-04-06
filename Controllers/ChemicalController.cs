using LifeSciencesHackathon.Model;
using LifeSciencesHackathon.Service;
using Microsoft.AspNetCore.Mvc;

namespace LifeSciencesHackathon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChemicalController : ControllerBase
    {
        private readonly ChemicalService _chemicalService;

        public ChemicalController(ChemicalService chemicalService)
        {
            _chemicalService = chemicalService;
        }

        [HttpGet("{name}")]
        public async Task<ActionResult<ChemicalInfo>> Get(string name)
        {
            var result = await _chemicalService.GetChemicalInfoAsync(name);
            if (result == null)
                return NotFound("Chemical not found.");

            return Ok(result);
        }

        [HttpGet("hazards/{name}")]
        public async Task<IActionResult> GetHazards(string name)
        {
            var basicInfo = await _chemicalService.GetChemicalInfoAsync(name);
            if (basicInfo == null)
                return NotFound("Chemical not found.");

            var hazardInfo = await _chemicalService.GetHazardInfoAsync(basicInfo.CID);
            if (hazardInfo == null)
                return NotFound("Hazard data not found.");

            return Ok(new
            {
                basicInfo.Name,
                basicInfo.MolecularFormula,
                basicInfo.MolecularWeight,
                basicInfo.ImageUrl,
                Hazards = hazardInfo.GhsHazards,
                SignalWord = hazardInfo.SignalWord,
                Synonyms = hazardInfo.Synonyms.Take(10)
            });
        }
    }
}
