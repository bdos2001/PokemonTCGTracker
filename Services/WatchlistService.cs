using System.Text.Json;

namespace PokemonTCGTracker.Services
{
    public class WatchlistService
    {
        private readonly string _path;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public WatchlistService(IWebHostEnvironment env)
        {
            var dir = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(dir);
            _path = Path.Combine(dir, "watchlist.json");
        }

        public async Task<List<string>> GetAsync()
        {
            await _lock.WaitAsync();
            try { return await ReadAsync(); }
            finally { _lock.Release(); }
        }

        public async Task AddAsync(string id)
        {
            await _lock.WaitAsync();
            try
            {
                var ids = await ReadAsync();
                if (ids.Contains(id)) return;
                ids.Add(id);
                await WriteAsync(ids);
            }
            finally { _lock.Release(); }
        }

        public async Task RemoveAsync(string id)
        {
            await _lock.WaitAsync();
            try
            {
                var ids = await ReadAsync();
                if (ids.Remove(id)) await WriteAsync(ids);
            }
            finally { _lock.Release(); }
        }

        private async Task<List<string>> ReadAsync()
        {
            if (!File.Exists(_path)) return new List<string>();
            try
            {
                await using var s = File.OpenRead(_path);
                return await JsonSerializer.DeserializeAsync<List<string>>(s) ?? new List<string>();
            }
            catch (JsonException) { return new List<string>(); }
        }

        private async Task WriteAsync(List<string> ids)
        {
            await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(ids, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
