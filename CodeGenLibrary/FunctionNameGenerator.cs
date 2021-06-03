﻿using System.Collections.Generic;
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
        sealed class NServiceBusEndpointNameAttribute : System.Attribute
        {
            public string Name { get; }
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
            public readonly List<string> Names = new();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                // find all valid attributes
                if (context.Node is AttributeSyntax attributeSyntax
                    && attributeSyntax.ArgumentList?.Arguments.Count == 1
                    && context.SemanticModel.GetTypeInfo(attributeSyntax).Type?.ToDisplayString() == "NServiceBusEndpointNameAttribute")
                {
                    var name = context.SemanticModel.GetConstantValue(attributeSyntax.ArgumentList.Arguments[0].Expression).ToString();
                    Names.Add(name);
                }
            }
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var rx = (SyntaxReceiver)context.SyntaxContextReceiver!;
            foreach (var name in rx!.Names)
            {
                var source =
$@"
public partial class FunctionEndpointTrigger {{
        public const string Name = ""{name}"";
}}
";
                context.AddSource("NServiceBus__FunctionEndpointTrigger", SourceText.From(source, Encoding.UTF8));
            }
        }
    }
}