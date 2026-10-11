using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator
{
    [Generator]
    public class MethodGenerator : IIncrementalGenerator
    {
        private const string AttributeText = @"
using System;
namespace MethodGenerator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [System.Diagnostics.Conditional(""MethodGenerator_DEBUG"")]
    sealed class AddMethodsAttribute : Attribute
    {
        public AddMethodsAttribute() { }
    }
}
";
        private const string AttributeFullName = "MethodGenerator.AddMethodsAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Emit the attribute definition into the compilation
            context.RegisterPostInitializationOutput(static ctx =>
                ctx.AddSource("AddMethodsAttribute.g.cs", SourceText.From(AttributeText, Encoding.UTF8)));

            // ForAttributeWithMetadataName: highly optimised — only visits nodes that actually
            // carry the target attribute, avoiding a full syntax-tree scan on every keystroke.
            var classProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    AttributeFullName,
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol)
                .Where(static t => t is not null);

            context.RegisterSourceOutput(classProvider, static (spc, typeSymbol) =>
            {
                string? source = ProcessClass(typeSymbol);
                if (source is not null)
                    spc.AddSource($"Generated_{typeSymbol.Name}.g.cs", SourceText.From(source, Encoding.UTF8));
            });
        }

        private static string? ProcessClass(INamedTypeSymbol classSymbol)
        {
            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
                return null;

            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            return $@"
namespace {namespaceName}
{{
    public partial class {classSymbol.Name}
    {{
        public int Default() {{ return 100; }}
{GenerateAdditionalMethods()}
    }}
}}";
        }

        private static string GenerateAdditionalMethods() => @"
        public void LowerToUpper()
        {
            Text = Text.ToUpper();
        }

        public void NextPage()
        {
            Page = Page + 1;
        }

        public void GeneratedMethod()
        {
            System.Console.WriteLine(""Invocation of Generated Method!"");
            System.Console.WriteLine();
            System.Console.WriteLine($""Values are:  Page = { Page } and Text = { Text }"");
        }";
    }
}
