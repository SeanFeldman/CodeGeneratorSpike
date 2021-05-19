using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeGenLibrary
{
    [Generator]
    public class FunctionNameGenerator : ISourceGenerator
    {
        private const string attributeSource = @"
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=false)]
    internal sealed class FunctionNameAttribute: System.Attribute
    {
        public string Name { get; }
        public FunctionNameAttribute(string name) => Name = name;
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
            context.RegisterForPostInitialization(pi => pi.AddSource("FunctionName_MainAttributes__", attributeSource));
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        class SyntaxReceiver : ISyntaxContextReceiver
        {
            public readonly List<string> Names = new();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                // find all valid attributes
                if (context.Node is AttributeSyntax attributeSyntax
                    && attributeSyntax.ArgumentList?.Arguments.Count == 1
                    && context.SemanticModel.GetTypeInfo(attributeSyntax).Type?.ToDisplayString() == "FunctionNameAttribute")
                {
                    var name = context.SemanticModel.GetConstantValue(attributeSyntax.ArgumentList.Arguments[0].Expression).ToString();
                    Names.Add(name);
                }
            }
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var rx = (SyntaxReceiver)context.SyntaxReceiver!;
            foreach (var name in rx!.Names)
            {
                var source =
                    $@"
namespace FunctionName {{
    public static partial class Endpoint {{
        public const string Name = {name}"";
    }}
}}
";
                context.AddSource($"FunctionName{name}", SourceText.From(source, Encoding.UTF8));
            }
        }
    }
}