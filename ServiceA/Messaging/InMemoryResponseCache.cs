using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;

namespace ServiceA.Messaging;

public interface IResponseCache
{
    void MarkPending(string key);
    void Store(string key, IReadOnlyList<string> values);
    bool TryGet(string key, out IReadOnlyList<string> values);
}

public class InMemoryResponseCache : IResponseCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _entries = new();

    public void MarkPending(string key)
    {
        _entries[key] = CacheEntry.Pending;
    }

    public void Store(string key, IReadOnlyList<string> values)
    {
        _entries[key] = new CacheEntry(values);
    }

    public bool TryGet(string key, out IReadOnlyList<string> values)
    {
        values = Array.Empty<string>();
        if (!_entries.TryGetValue(key, out var entry))
        {
            return false;
        }

        if (entry.IsPending)
        {
            return false;
        }

        values = entry.Values;
        return true;
    }

    private readonly record struct CacheEntry
    {
        public CacheEntry()
        {
            Values = Array.Empty<string>();
            IsPending = true;
        }

        public CacheEntry(IReadOnlyList<string> values)
        {
            Values = values;
            IsPending = false;
        }

        public IReadOnlyList<string> Values { get; } = Array.Empty<string>();

        public bool IsPending { get; } = true;

        public static CacheEntry Pending => new();
    }
}

