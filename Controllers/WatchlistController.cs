using Microsoft.AspNetCore.Mvc;
using PokemonTCGTracker.Services;

namespace PokemonTCGTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WatchlistController : ControllerBase
    {
        private readonly WatchlistService _watchlist;

        public WatchlistController(WatchlistService watchlist)
        {
            _watchlist = watchlist;
        }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _watchlist.GetAsync());

        [HttpPost("{id}")]
        public async Task<IActionResult> Add(string id)
        {
            await _watchlist.AddAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(string id)
        {
            await _watchlist.RemoveAsync(id);
            return NoContent();
        }
    }
}
