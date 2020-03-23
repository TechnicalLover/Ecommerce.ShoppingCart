namespace ShoppingCartService.Services
{
    using System;

    public interface ICache
    {
        void Add(string key, object value, TimeSpan timeToLife);
        object Get(string key);
    }
}