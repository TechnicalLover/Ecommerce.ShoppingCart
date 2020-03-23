namespace ShoppingCartService.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class InmemoryCache : ICache
    {
        private static IDictionary<string, Tuple<DateTimeOffset, object>> cache = new ConcurrentDictionary<string, Tuple<DateTimeOffset, object>>();

        public void Add(string key, object value, TimeSpan timeToLife)
        {
            cache[key] = Tuple.Create(DateTimeOffset.UtcNow.Add(timeToLife), value);
        }

        public object Get(string key)
        {
            Tuple<DateTimeOffset, object> value;
            if (cache.TryGetValue(key, out value) && value.Item1 > DateTimeOffset.UtcNow)
            {
                // value is still valid
                return value;
            }
            // value is life time is expired
            cache.Remove(key);
            return null;
        }
    }
}