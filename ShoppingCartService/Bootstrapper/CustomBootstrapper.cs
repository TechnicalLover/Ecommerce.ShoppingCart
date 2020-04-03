namespace ShoppingCartService
{
    using System;
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.Bootstrappers.Autofac;
    using Nancy.Configuration;
    using Nancy.Responses.Negotiation;
    using ShoppingCartService.EventFeed;
    using ShoppingCartService.Services;
    using ShoppingCartService.Models.Configurations;
    using ShoppingCartService.ShoppingCart;
    using Nancy.Owin;
    using Serilog;

    public class CustomBootstrapper : AutofacNancyBootstrapper
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public CustomBootstrapper(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            // we have GlobalErrorMiddleware to catch all unhandled exceptions
            //pipelines.OnError += OnError;
        }

        protected override void ConfigureApplicationContainer(ILifetimeScope container)
        {
            base.ConfigureApplicationContainer(container);

            container.Configure<ShoppingCartStoreConfig>(_configuration.GetSection("ShoppingCartStore"));
            container.Update(builder => builder.RegisterType<ShoppingCartStore>().As<IShoppingCartStore>());

            container.Configure<ProductCatalogClientConfig>(_configuration.GetSection("ProductCatalogClient"));
            container.Update(builder => builder.RegisterType<ProductCatalogClient>().As<IProductCatalogClient>());
            container.Update(builder => builder.RegisterType<InmemoryCache>().As<ICache>());

            container.Configure<EventStoreConfig>(_configuration.GetSection("EventStore"));
            container.Update(builder => builder.Register<IEventStore>(context =>
            {
                var config = context.Resolve<EventStoreConfig>();
                switch (config.StorageOption)
                {
                    case StorageOption.EventStore:
                        {
                            return new EventStore(config);
                        }
                    default:
                        {
                            return new SqlEventStore(config);
                        }
                }
            }));

            container.Update(builder => builder.RegisterInstance(_logger).As<ILogger>());
        }

        protected override void ConfigureRequestContainer(ILifetimeScope container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            // get correlation token from owin context which had been added before by CorrelationTokenMiddleware
            var correlationToken = context.GetOwinEnvironment()["correlationToken"] as string;
            container.Update(builder => builder.RegisterInstance(new HttpClientFactory(correlationToken)).As<IHttpClientFactory>());
        }

        public override void Configure(INancyEnvironment env)
        {
            env.Tracing(enabled: true, displayErrorTraces: true);
        }

        private Response OnError(NancyContext context, Exception ex)
        {
            _logger.Error(ex, "An unhandled error occured.");
            var negotiator = ApplicationContainer.Resolve<IResponseNegotiator>();
            return negotiator.NegotiateResponse(new
            {
                Error = "Internal error. Please see server log for details"
            }, context);
        }
    }
}
