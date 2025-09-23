using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Community.PowerToys.Run.Plugin.CheatSheets;

/// <summary>
/// Simple in-memory cache implementation used by the plugin to cache HTTP responses.
/// </summary>
public sealed class CacheService : IDisposable
{
    private readonly ConcurrentDictionary<string, CacheItem> _cache;
    private readonly Timer _cleanupTimer;
    private bool _disposed;

    public CacheService()
    {
        _cache = new ConcurrentDictionary<string, CacheItem>();
        
        // Clean up expired items every 5 minutes
        _cleanupTimer = new Timer(CleanupExpiredItems, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public T Get<T>(string key) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        if (_cache.TryGetValue(key, out var item))
        {
            if (item.IsExpired)
            {
                _cache.TryRemove(key, out _);
                return null;
            }

            return item.Value as T;
        }

        return null;
    }

    public void Set<T>(string key, T value, TimeSpan expiration) where T : class
    {
        if (string.IsNullOrWhiteSpace(key) || value is null)
        {
            return;
        }

        var expiry = DateTimeOffset.Now.Add(expiration);
        var cacheItem = new CacheItem(value, expiry);
        
        _cache.AddOrUpdate(key, cacheItem, (_, _) => cacheItem);
    }

    private void CleanupExpiredItems(object state)
    {
        var keysToRemove = new List<string>();
        
        foreach (var kvp in _cache)
        {
            if (kvp.Value.IsExpired)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _cache.TryRemove(key, out _);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _cleanupTimer?.Dispose();
        _cache.Clear();
        _disposed = true;
    }

    private sealed class CacheItem
    {
        public object Value { get; }
        public DateTimeOffset Expiry { get; }
        public bool IsExpired => DateTimeOffset.Now > Expiry;

        public CacheItem(object value, DateTimeOffset expiry)
        {
            Value = value;
            Expiry = expiry;
        }
    }
}