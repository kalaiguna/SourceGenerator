using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator
{
    [Generator]
    public class PropertyGenerator : IIncrementalGenerator
    {
        private const string AttributeText = @"
using System;
namespace PropertyGenerator
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    [System.Diagnostics.Conditional(""PropertyGenerator_DEBUG"")]
    sealed class AddPropertyAttribute : Attribute
    {
        public AddPropertyAttribute() { }
    }
}
";
        private const string AttributeFullName = "PropertyGenerator.AddPropertyAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Emit the attribute definition into the compilation
            context.RegisterPostInitializationOutput(static ctx =>
                ctx.AddSource("AddPropertyAttribute.g.cs", SourceText.From(AttributeText, Encoding.UTF8)));

            // Collect all fields carrying [AddProperty], then group by declaring type.
            // ForAttributeWithMetadataName scans only nodes with the attribute —
            // far cheaper than a full ISyntaxContextReceiver walk.
            var fieldProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    AttributeFullName,
                    predicate: static (node, _) => node is VariableDeclaratorSyntax,
                    transform: static (ctx, _) => (IFieldSymbol)ctx.TargetSymbol)
                .Where(static f => f is not null)
                .Collect();

            context.RegisterSourceOutput(fieldProvider, static (spc, fields) =>
            {
                foreach (var group in fields.GroupBy(f => f.ContainingType.ToDisplayString()))
                {
                    var typeSymbol = group.First().ContainingType;
                    string? source = ProcessClass(typeSymbol, group.ToList());
                    if (source is not null)
                        spc.AddSource($"Generated_{typeSymbol.Name}.g.cs", SourceText.From(source, Encoding.UTF8));
                }
            });
        }

        private static string? ProcessClass(INamedTypeSymbol classSymbol, List<IFieldSymbol> fields)
        {
            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
                return null;

            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            var source = new StringBuilder($@"
namespace {namespaceName}
{{
    public partial class {classSymbol.Name}
    {{
");
            foreach (var field in fields)
                ProcessField(source, field);

            source.Append("    }\n}");
            return source.ToString();
        }

        private static void ProcessField(StringBuilder source, IFieldSymbol fieldSymbol)
        {
            string propertyName = ChooseName(fieldSymbol.Name);
            if (propertyName.Length == 0 || propertyName == fieldSymbol.Name)
                return;

            source.Append($@"
        public {fieldSymbol.Type} {propertyName}
        {{
            get => this.{fieldSymbol.Name};
            set => this.{fieldSymbol.Name} = value;
        }}
");
        }

        private static string ChooseName(string fieldName)
        {
            fieldName = fieldName.TrimStart('_');
            if (fieldName.Length == 0) return string.Empty;
            if (fieldName.Length == 1) return fieldName.ToUpper();
            return char.ToUpper(fieldName[0]) + fieldName.Substring(1);
        }
    }
}
