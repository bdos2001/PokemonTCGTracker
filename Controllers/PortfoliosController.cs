using Microsoft.AspNetCore.Mvc;
using PokemonTCGTracker.Models;
using PokemonTCGTracker.Services;

namespace PokemonTCGTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfoliosController : ControllerBase
    {
        private readonly PortfolioService _portfolios;

        public PortfoliosController(PortfolioService portfolios)
        {
            _portfolios = portfolios;
        }

        [HttpGet]
        public async Task<IActionResult> List() => Ok(await _portfolios.ListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var p = await _portfolios.GetAsync(id);
            return p == null ? NotFound() : Ok(p);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NameRequest req)
        {
            var name = req.Name?.Trim() ?? "";
            if (name.Length is 0 or > 60) return BadRequest("O nome é obrigatório (máx. 60 caracteres).");
            var p = await _portfolios.CreateAsync(name);
            return Ok(new PortfolioSummary { Id = p.Id, Name = p.Name, Count = 0 });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Rename(string id, [FromBody] NameRequest req)
        {
            var name = req.Name?.Trim() ?? "";
            if (name.Length is 0 or > 60) return BadRequest("O nome é obrigatório (máx. 60 caracteres).");
            var p = await _portfolios.RenameAsync(id, name);
            return p == null ? NotFound() : NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id) =>
            await _portfolios.DeleteAsync(id) ? NoContent() : NotFound();

        [HttpPost("{id}/entries")]
        public async Task<IActionResult> AddEntry(string id, [FromBody] EntryRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.CardId)) return BadRequest("cardId é obrigatório.");
            if (Invalid(req.PricePaid, req.Condition, req.Note, out var error)) return BadRequest(error);
            var e = await _portfolios.AddEntryAsync(id, req with { CardId = req.CardId.Trim() });
            return e == null ? NotFound() : Ok(e);
        }

        [HttpPut("{id}/entries/{entryId}")]
        public async Task<IActionResult> UpdateEntry(string id, string entryId, [FromBody] EntryUpdateRequest req)
        {
            if (Invalid(req.PricePaid, req.Condition, req.Note, out var error)) return BadRequest(error);
            var e = await _portfolios.UpdateEntryAsync(id, entryId, req);
            return e == null ? NotFound() : Ok(e);
        }

        [HttpDelete("{id}/entries/{entryId}")]
        public async Task<IActionResult> RemoveEntry(string id, string entryId) =>
            await _portfolios.RemoveEntryAsync(id, entryId) ? NoContent() : NotFound();

        [HttpPost("{id}/entries/{entryId}/move")]
        public async Task<IActionResult> MoveEntry(string id, string entryId, [FromBody] MoveRequest req) =>
            await _portfolios.MoveEntryAsync(id, entryId, req.TargetId) ? NoContent() : NotFound();

        private static bool Invalid(decimal price, string condition, string? note, out string error)
        {
            error = "";
            if (price < 0) error = "O preço não pode ser negativo.";
            else if (!PortfolioService.Conditions.Contains(condition)) error = "Estado de conservação inválido.";
            else if (note?.Length > 500) error = "A observação é demasiado longa (máx. 500 caracteres).";
            return error != "";
        }
    }
}
