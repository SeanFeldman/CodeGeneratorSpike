using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeGenLibrary
{
    [Generator]
    public class FunctionEndpointTriggerGenerator : ISourceGenerator
    {
        private const string attributeSource = @"// Auto-generated class to provide the queue name to the auto-generated NServiceBus trigger function
///<summary>
///Assembly attribute to specify NServiceBus logical endpoint name.
///This name is used to wire-up an auto-generated service bus trigger function, responding to messages in the queue specified by the name provided.
///</summary>
[System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=false)]
sealed class NServiceBusEndpointNameAttribute : System.Attribute
{
    public string Name { get; }
    ///<summary>
    ///Endpoint logical name.
    ///</summary>
    ///<param name=""name"">Endpoint name that is the input queue name.</param>
    public NServiceBusEndpointNameAttribute(string name) => Name = name;
}
";

        public void Initialize(GeneratorInitializationContext context)
        {
            // #if DEBUG
            //             if (!Debugger.IsAttached)
            //             {
            //                 Debugger.Launch();
            //             }
            // #endif

            context.RegisterForPostInitialization(pi => pi.AddSource("NServiceBus__NServiceBusEndpointNameAttribute", attributeSource));

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        class SyntaxReceiver : ISyntaxContextReceiver
        {
            internal string EndpointName;

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                // find all valid attributes
                if (context.Node is AttributeSyntax attributeSyntax
                    && attributeSyntax.ArgumentList?.Arguments.Count == 1
                    && context.SemanticModel.GetTypeInfo(attributeSyntax).Type?.ToDisplayString() == "NServiceBusEndpointNameAttribute")
                {
                    EndpointName = context.SemanticModel.GetConstantValue(attributeSyntax.ArgumentList.Arguments[0].Expression).ToString();
                }
            }
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxReceiver = (SyntaxReceiver)context.SyntaxContextReceiver!;

            var source =
$@"// Auto-generated class serving as a trigger function for NServiceBus
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using NServiceBus;

class FunctionEndpointTrigger
{{
    readonly IFunctionEndpoint endpoint;

    public FunctionEndpointTrigger(IFunctionEndpoint endpoint)
    {{
        this.endpoint = endpoint;
    }}

    [FunctionName(""NServiceBusFunctionEndpointTrigger"")]
    public async Task Run(
        [ServiceBusTrigger(queueName: ""{syntaxReceiver!.EndpointName}"")]
        Message message,
        ILogger logger,
        ExecutionContext executionContext)
    {{
        await endpoint.Process(message, executionContext, logger);
    }}
}}
";
            context.AddSource("NServiceBus__FunctionEndpointTrigger", SourceText.From(source, Encoding.UTF8));
        }
    }
}