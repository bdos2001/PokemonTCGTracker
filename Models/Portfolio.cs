namespace PokemonTCGTracker.Models
{
    public class Portfolio
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public List<PortfolioEntry> Entries { get; set; } = new();
    }

    // cada compra é uma entrada (a mesma carta pode aparecer várias vezes porque pode ter condições diferentes).
    public class PortfolioEntry
    {
        public string EntryId { get; set; } = "";
        public string CardId { get; set; } = "";
        public decimal PricePaid { get; set; }
        public string Condition { get; set; } = "Near Mint";
        public string? Note { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }

    public class PortfolioSummary
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int Count { get; set; }
        public string? CoverCardId { get; set; }
    }

    public record NameRequest(string Name);
    public record EntryRequest(string CardId, decimal PricePaid, string Condition, string? Note = null);
    public record EntryUpdateRequest(decimal PricePaid, string Condition, string? Note = null);
    public record MoveRequest(string TargetId);
}
