namespace PokemonTCGTracker.Models
{
    public class TcgdexCardBrief
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Image { get; set; }
    }
    public class TcgdexCard
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Image { get; set; }
        public string? Rarity { get; set; }
        public TcgdexSet? Set { get; set; }
        public TcgdexPricing? Pricing { get; set; }
    }

    public class TcgdexSetBrief
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class TcgdexSetDetail
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public List<TcgdexCardBrief> Cards { get; set; } = new();
    }

    public class TcgdexSet { public string Name { get; set; } = ""; }
    public class TcgdexPricing { public TcgdexCardmarket? Cardmarket { get; set; } }
    public class TcgdexCardmarket
    {
        public decimal? Trend { get; set; }
        public decimal? Avg30 { get; set; }
    }
}