namespace PokemonTCGTracker.Models
{
    public class Card
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string SetName { get; set; } = "";
        public string Rarity { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public decimal? TrendPrice { get; set; }
        public decimal? AvgLast30DaysSold { get; set; }
    }
}