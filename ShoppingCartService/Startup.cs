using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nancy.Owin;
using Serilog;
using Serilog.Events;
using ShoppingCartService.Middlewares;
using ShoppingCartService.Models.Configurations;
using ShoppingCartService.Models.Constants;

namespace ShoppingCartService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Serilog package include a HTTP request logging middleware
            // app.UseSerilogRequestLogging(options =>
            // {
            //     // customize the message template
            //     options.MessageTemplate = LoggingTemplate.RequestTemplate;

            //     // Emit debug-level events instead of the defaults
            //     options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

            //     // Attach additional properties to the request completion event
            //     options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            //     {
            //         diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            //         diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            //     };
            // });

            var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            app.UseOwin(buildFunc =>
            {
                buildFunc(next => new MonitoringMiddleware(next, HealthCheck).Invoke);
                buildFunc.UseNancy(opt => opt.Bootstrapper = new CustomBootstrapper(Configuration, loggerFactory));
            });
        }

        /// <summary>
        /// Health check function, perform a database query and return "true" if success.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> HealthCheck()
        {
            var healthCheckConfig = new HealthCheckConfig();
            Configuration.GetSection("HealthCheck").Bind(healthCheckConfig);
            using (var conn = new SqlConnection(healthCheckConfig.ConnectionString))
            {
                var count = (await conn.QueryAsync<int>("SELECT COUNT(ID) FROM shopping_cart")).Single();
                return count > healthCheckConfig.Threshold;
            }
        }
    }
}
