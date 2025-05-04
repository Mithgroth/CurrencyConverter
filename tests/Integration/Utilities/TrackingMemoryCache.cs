using Microsoft.Extensions.Caching.Memory;

namespace Integration.Utilities;

/// <summary>
/// A simple memory cache wrapper that tracks hits and misses.
/// For testing purposes only.
/// </summary>
public class TrackingMemoryCache : IMemoryCache
{
    private readonly MemoryCache _inner = new(new MemoryCacheOptions());
    public int Hits { get; private set; }
    public int Misses { get; private set; }

    public bool TryGetValue(object key, out object value)
    {
        var found = _inner.TryGetValue(key, out value);
        if (found)
        {
            Hits++;
        }
        else
        {
            Misses++;
        }

        return found;
    }

    public ICacheEntry CreateEntry(object key) => _inner.CreateEntry(key);
    public void Remove(object key) => _inner.Remove(key);
    public void Dispose() => _inner.Dispose();
}
