namespace ShoppingCartService
{
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using Nancy.Bootstrappers.Autofac;

    internal static class ContainerExtensions
    {
        public static void Configure<TOptions>(this ILifetimeScope container, IConfiguration configuration)
            where TOptions : class, new()
        {
            var options = new TOptions();
            configuration.Bind(options);
            container.Update(builder => builder.RegisterInstance(options).As<TOptions>());
        }
    }
}