using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace FunctionApp
{
    public class MessageGenerator
    {
        readonly IFunctionEndpoint functionEndpoint;

        public MessageGenerator(IFunctionEndpoint functionEndpoint)
        {
            this.functionEndpoint = functionEndpoint;
        }

        [FunctionName("MessageGenerator")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest request, ExecutionContext executionContext, ILogger logger)
        {
            logger.LogInformation("C# HTTP trigger function received a request.");

            var sendOptions = new SendOptions();
            sendOptions.RouteToThisEndpoint();

            await functionEndpoint.Send(new TestMessage(), sendOptions, executionContext, logger);

            return new OkObjectResult($"{nameof(TestMessage)} sent.");
        }
    }
}