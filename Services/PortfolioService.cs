using System.Text.Json;
using PokemonTCGTracker.Models;

namespace PokemonTCGTracker.Services
{
    // Um ficheiro JSON por portefólio: Data/portfolios/{id}.json
    public class PortfolioService
    {
        public static readonly string[] Conditions =
            { "Mint", "Near Mint", "Excellent", "Good", "Light Played", "Played", "Poor" };

        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };
        private readonly string _dir;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public PortfolioService(IWebHostEnvironment env)
        {
            _dir = Path.Combine(env.ContentRootPath, "Data", "portfolios");
            Directory.CreateDirectory(_dir);
        }

        public async Task<List<PortfolioSummary>> ListAsync()
        {
            await _lock.WaitAsync();
            try
            {
                var list = new List<(Portfolio P, DateTime Created)>();
                foreach (var file in Directory.EnumerateFiles(_dir, "*.json"))
                {
                    var p = await ReadFileAsync(file);
                    if (p != null) list.Add((p, File.GetCreationTimeUtc(file)));
                }
                return list.OrderBy(x => x.Created).Select(x => new PortfolioSummary
                {
                    Id = x.P.Id,
                    Name = x.P.Name,
                    Count = x.P.Entries.Count,
                    CoverCardId = x.P.Entries.OrderByDescending(e => e.PricePaid).FirstOrDefault()?.CardId
                }).ToList();
            }
            finally { _lock.Release(); }
        }

        public async Task<Portfolio?> GetAsync(string id)
        {
            await _lock.WaitAsync();
            try { return await ReadAsync(id); }
            finally { _lock.Release(); }
        }

        public async Task<Portfolio> CreateAsync(string name)
        {
            await _lock.WaitAsync();
            try
            {
                var p = new Portfolio { Id = Guid.NewGuid().ToString("N"), Name = name };
                await WriteAsync(p);
                return p;
            }
            finally { _lock.Release(); }
        }

        public Task<Portfolio?> RenameAsync(string id, string name) =>
            MutateAsync(id, p => { p.Name = name; return true; });

        public async Task<bool> DeleteAsync(string id)
        {
            await _lock.WaitAsync();
            try
            {
                var path = PathFor(id);
                if (path == null || !File.Exists(path)) return false;
                File.Delete(path);
                return true;
            }
            finally { _lock.Release(); }
        }

        public async Task<PortfolioEntry?> AddEntryAsync(string id, EntryRequest req)
        {
            PortfolioEntry? entry = null;
            await MutateAsync(id, p =>
            {
                entry = new PortfolioEntry
                {
                    EntryId = Guid.NewGuid().ToString("N"),
                    CardId = req.CardId,
                    PricePaid = req.PricePaid,
                    Condition = req.Condition,
                    Note = CleanNote(req.Note)
                };
                p.Entries.Add(entry);
                return true;
            });
            return entry;
        }

        public async Task<PortfolioEntry?> UpdateEntryAsync(string id, string entryId, EntryUpdateRequest req)
        {
            PortfolioEntry? entry = null;
            await MutateAsync(id, p =>
            {
                entry = p.Entries.FirstOrDefault(e => e.EntryId == entryId);
                if (entry == null) return false;
                entry.PricePaid = req.PricePaid;
                entry.Condition = req.Condition;
                entry.Note = CleanNote(req.Note);
                return true;
            });
            return entry;
        }

        public async Task<bool> RemoveEntryAsync(string id, string entryId)
        {
            var found = false;
            await MutateAsync(id, p => found = p.Entries.RemoveAll(e => e.EntryId == entryId) > 0);
            return found;
        }

        public async Task<bool> MoveEntryAsync(string id, string entryId, string targetId)
        {
            if (id == targetId) return false;
            await _lock.WaitAsync();
            try
            {
                var from = await ReadAsync(id);
                var to = await ReadAsync(targetId);
                var entry = from?.Entries.FirstOrDefault(e => e.EntryId == entryId);
                if (from == null || to == null || entry == null) return false;
                to.Entries.Add(entry);          // grava primeiro no destino: nunca se perde a carta
                await WriteAsync(to);
                from.Entries.Remove(entry);
                await WriteAsync(from);
                return true;
            }
            finally { _lock.Release(); }
        }

        private static string? CleanNote(string? note)
        {
            note = note?.Trim();
            return string.IsNullOrEmpty(note) ? null : note;
        }

        private async Task<Portfolio?> MutateAsync(string id, Func<Portfolio, bool> change)
        {
            await _lock.WaitAsync();
            try
            {
                var p = await ReadAsync(id);
                if (p == null) return null;
                if (change(p)) await WriteAsync(p);
                return p;
            }
            finally { _lock.Release(); }
        }

        // ids são GUIDs: impede caminhos arbitrários
        private string? PathFor(string id) =>
            Guid.TryParseExact(id, "N", out _) ? Path.Combine(_dir, id + ".json") : null;

        private async Task<Portfolio?> ReadAsync(string id)
        {
            var path = PathFor(id);
            return path == null ? null : await ReadFileAsync(path);
        }

        private static async Task<Portfolio?> ReadFileAsync(string path)
        {
            if (!File.Exists(path)) return null;
            try
            {
                await using var s = File.OpenRead(path);
                return await JsonSerializer.DeserializeAsync<Portfolio>(s);
            }
            catch (JsonException) { return null; }
        }

        private async Task WriteAsync(Portfolio p)
        {
            await File.WriteAllTextAsync(PathFor(p.Id)!, JsonSerializer.Serialize(p, JsonOpts));
        }
    }
}
