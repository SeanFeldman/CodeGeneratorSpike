using FunctionApp;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using NServiceBus;

[assembly: FunctionsStartup(typeof(Startup))]
[assembly: NServiceBusEndpointName(Startup.EndpointName)]

namespace FunctionApp
{
    public class Startup : FunctionsStartup
    {
        internal const string EndpointName = "sales";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.UseNServiceBus(() => new ServiceBusTriggeredEndpointConfiguration(Startup.EndpointName));
        }
    }
}