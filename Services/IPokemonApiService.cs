using PokemonTCGTracker.Models;

namespace PokemonTCGTracker.Services
{
    public interface IPokemonApiService
    {
        Task<List<Card>> SearchCardsAsync(string name);
        Task<List<Card>> SearchCardsBySetAsync(string name);
        Task<Card?> GetCardAsync(string id);
    }
}