using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SourceGenerator
{
    [Generator]
    public class MethodGenerator : ISourceGenerator
    {
        private const string attributeText = @"
        using System;
        namespace GeneratedMethods
        {
            [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
            [System.Diagnostics.Conditional(""MethodGenerator_DEBUG"")]
            sealed class AddMethodsAttribute : Attribute
            {
                public AddMethodsAttribute()
                {
                }
            }
        }
        ";

        public void Execute(GeneratorExecutionContext context)
        {
            // retrieve the populated receiver 
            if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
                return;

            // get the added attribute 
            INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName("GeneratedMethods.AddMethodsAttribute");

            // generate the source for each class
            foreach (var type in receiver.Types)
            {
                string classSource = ProcessClass(type, attributeSymbol, context);
                context.AddSource($"Generated_{type.Name}.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private string ProcessClass(INamedTypeSymbol classSymbol, ISymbol attributeSymbol, GeneratorExecutionContext context)
        {

            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                return null; //TODO: issue a diagnostic that it must be top level
            }

            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            // begin building the generated source
            StringBuilder source = new StringBuilder($@"
            namespace {namespaceName}
            {{
                public partial class {classSymbol.Name} 
                {{
            ");

            // add Method to the partial class
            source.Append(" public int Default() { return 100; } ");
            source.Append(GenerateAdditionalMethods());
            source.Append("} }");
            
            return source.ToString();
        }

        private string GenerateAdditionalMethods()
        {
            var code = @"
            public void LowerToUpper() { 
                Text = Text.ToUpper(); 
            }
            public void NextPage() { 
                Page = Page + 1; 
            }
            public void GeneratedMethod()
            {
                System.Console.WriteLine(""Invocation of Generated  Method!"");
                System.Console.WriteLine();
                System.Console.WriteLine($""Values are:  Page = { Page } and Text = { Text }""); 
            }
            ";
            return code;
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //Debugger.Launch();   // uncomment this line to be able to debug the source generator
            // Register the attribute source
            context.RegisterForPostInitialization((i) => i.AddSource("AddMethodsAttribute", attributeText));

            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        class SyntaxReceiver : ISyntaxContextReceiver
        {
            public List<INamedTypeSymbol> Types { get; } = new List<INamedTypeSymbol>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                // any class with at least one attribute is a candidate for method generation
                if (context.Node is ClassDeclarationSyntax classDeclarationSyntax
                    && classDeclarationSyntax.AttributeLists.Count > 0)
                {
                    // Get the symbol being declared by the class, and keep it if its annotated
                    INamedTypeSymbol typeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
                    if (typeSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == "GeneratedMethods.AddMethodsAttribute"))
                    {
                        Types.Add(typeSymbol);
                    }
                }
            }
        }
    }
}
