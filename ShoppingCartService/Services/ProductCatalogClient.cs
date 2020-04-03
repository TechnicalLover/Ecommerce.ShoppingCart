namespace ShoppingCartService.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System;
    using Newtonsoft.Json;
    using Polly;
    using ShoppingCartService.Models.Configurations;
    using ShoppingCartService.Models.Dto;
    using ShoppingCartService.Models.MetaModels;
    using ShoppingCartService.Services.Response;

    public class ProductCatalogClient : IProductCatalogClient
    {
        private readonly string _productCatalogBaseUrl;
        private readonly string _getProductPathTemplate = "/products?productIds=[{0}]";
        private readonly ICache _cache;
        private readonly ShoppingCartService.IHttpClientFactory _httpClientFactory;

        public ProductCatalogClient(ProductCatalogClientConfig config, ShoppingCartService.IHttpClientFactory httpClientFactory, ICache cache)
        {
            if (config is null)
                throw new ArgumentNullException(nameof(config));
            _productCatalogBaseUrl = config.BaseUrl;
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private static IAsyncPolicy exponentialRetryPolicy =
            Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attemp => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attemp)),
                (ex, _) => Console.WriteLine(ex.ToString())
            );

        public Task<IEnumerable<Item>> GetShoppingCartItems(AddItem[] addItems) =>
            exponentialRetryPolicy
            .ExecuteAsync(async () =>
               await GetItemFromProductCatalogService(addItems)
               .ConfigureAwait(false));

        private async Task<IEnumerable<Item>> GetItemFromProductCatalogService(AddItem[] addItems)
        {
            int[] productIds = addItems.Select(item => item.ProductCode).ToArray();
            var response = await
            RequestProductFromApi(productIds)
                .ConfigureAwait(false);
            return await ConvertToItems(response, addItems)
                .ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> RequestProductFromApi(int[] productIds)
        {
            var productsResource = string.Format(
                _getProductPathTemplate, string.Join(",", productIds));
            var cachedResponse = this._cache.Get(productsResource) as HttpResponseMessage;
            if (cachedResponse == null)
            {
                using (var httpClient = _httpClientFactory.Create(new System.Uri(_productCatalogBaseUrl)))
                {
                    var response = await httpClient.GetAsync(productsResource).ConfigureAwait(false);
                    AddToCache(productsResource, response);
                    return response;
                }
            }
            return cachedResponse;
        }

        private void AddToCache(string resource, HttpResponseMessage response)
        {
            var cacheHeader = response
                .Headers
                .FirstOrDefault(h => h.Key == "cache-control");
            if (string.IsNullOrEmpty(cacheHeader.Key))
                return;
            var maxAge =
                CacheControlHeaderValue.Parse(cacheHeader.Value.ToString())
                .MaxAge;
            if (maxAge.HasValue)
                this._cache.Add(key: resource, value: resource, timeToLife: maxAge.Value);
        }

        private async Task<IEnumerable<Item>> ConvertToItems(HttpResponseMessage response, AddItem[] addItems)
        {
            response.EnsureSuccessStatusCode();
            var products = JsonConvert.DeserializeObject<List<ProductCatalogResponse>>(
                await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            return products.Select(product =>
            {
                var addItem = addItems.First(item => item.ProductCode == product.Id);
                var selectedFormat = product.GetFormat(addItem.UnitCode);
                var price = selectedFormat.GetRetailerPrice();
                return new Item(
                    product.Id,
                    product.ProductName,
                    selectedFormat.Unit.Id,
                    selectedFormat.Unit.UnitName,
                    price.Currency,
                    price.Amount,
                    addItem.Quantity,
                    product.Description);
            });
        }
    }
}