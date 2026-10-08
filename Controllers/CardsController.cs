using Microsoft.AspNetCore.Mvc;
using PokemonTCGTracker.Services;

namespace PokemonTCGTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly IPokemonApiService _pokemonApi;

        public CardsController(IPokemonApiService pokemonApi)
        {
            _pokemonApi = pokemonApi;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("O parâmetro 'name' é obrigatório.");

            var cards = await _pokemonApi.SearchCardsAsync(name);
            return Ok(cards);
        }

        [HttpGet("by-set")]
        public async Task<IActionResult> BySet([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("O parâmetro 'name' é obrigatório.");

            var cards = await _pokemonApi.SearchCardsBySetAsync(name);
            return Ok(cards);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var card = await _pokemonApi.GetCardAsync(id);
            return card == null ? NotFound() : Ok(card);
        }
    }
}