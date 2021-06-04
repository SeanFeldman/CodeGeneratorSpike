using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

namespace FunctionApp
{
    public class TestMessageHandler : IHandleMessages<TestMessage>
    {
        private static readonly ILog Log = LogManager.GetLogger<TestMessageHandler>();

        public Task Handle(TestMessage message, IMessageHandlerContext context)
        {
            Log.Warn("Received test message");

            return Task.CompletedTask;
        }
    }
}