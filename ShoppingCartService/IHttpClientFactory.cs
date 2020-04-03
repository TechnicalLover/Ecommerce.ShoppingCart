using System;
using System.Net.Http;

namespace ShoppingCartService
{
    public interface IHttpClientFactory
    {
        HttpClient Create(Uri uri);
    }
}