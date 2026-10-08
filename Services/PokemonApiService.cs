using System.Net;
using PokemonTCGTracker.Models;

namespace PokemonTCGTracker.Services
{
    public class PokemonApiService : IPokemonApiService
    {
        private readonly HttpClient _http;

        public PokemonApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Card>> SearchCardsAsync(string name)
        {
            var url = $"cards?name={Uri.EscapeDataString(name)}";
            var result = await _http.GetFromJsonAsync<List<TcgdexCardBrief>>(url);

            return result?.Select(c => new Card
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = BuildImageUrl(c.Image)
            }).ToList() ?? new List<Card>();
        }

        public async Task<List<Card>> SearchCardsBySetAsync(string name)
        {
            var sets = await _http.GetFromJsonAsync<List<TcgdexSetBrief>>($"sets?name={Uri.EscapeDataString(name)}")
                       ?? new List<TcgdexSetBrief>();

            var details = await Task.WhenAll(sets.Select(async s =>
            {
                try { return await _http.GetFromJsonAsync<TcgdexSetDetail>($"sets/{Uri.EscapeDataString(s.Id)}"); }
                catch (HttpRequestException) { return null; }
            }));

            return details
                .Where(d => d != null)
                .SelectMany(d => d!.Cards.Select(c => new Card
                {
                    Id = c.Id,
                    Name = c.Name,
                    SetName = d.Name,
                    ImageUrl = BuildImageUrl(c.Image)
                }))
                .ToList();
        }

        public async Task<Card?> GetCardAsync(string id)
        {
            try
            {
                var c = await _http.GetFromJsonAsync<TcgdexCard>($"cards/{Uri.EscapeDataString(id)}");
                if (c == null) return null;

                return new Card
                {
                    Id = c.Id,
                    Name = c.Name,
                    SetName = c.Set?.Name ?? "",
                    Rarity = c.Rarity ?? "",
                    ImageUrl = BuildImageUrl(c.Image),
                    TrendPrice = c.Pricing?.Cardmarket?.Trend,
                    AvgLast30DaysSold = c.Pricing?.Cardmarket?.Avg30
                };
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private static string BuildImageUrl(string? image)
        {
            return string.IsNullOrEmpty(image) ? "" : $"{image}/high.png";
        }
    }
}