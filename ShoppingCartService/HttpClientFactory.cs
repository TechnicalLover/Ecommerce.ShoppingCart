using System;
using System.Net.Http;

namespace ShoppingCartService
{
    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly string correlationToken;

        public HttpClientFactory(string correlationToken)
        {
            this.correlationToken = correlationToken;
        }

        HttpClient IHttpClientFactory.Create(Uri uri)
        {
            var client = new HttpClient() { BaseAddress = uri };
            client
                .DefaultRequestHeaders
                .Add("Correlation-Token", this.correlationToken);
            return client;
        }
    }
}