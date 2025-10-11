using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace JsonDerivedTypeGenerator;

[Generator]
public sealed class DerivedTypesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var typeDeclarations = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: (s, _) => s is InterfaceDeclarationSyntax or ClassDeclarationSyntax,
                transform: (ctx, _) => ctx.Node as TypeDeclarationSyntax
            )
            .Where(x => x != null);

        var compilationAndTypes = context.CompilationProvider.Combine(typeDeclarations.Collect());

        context.RegisterSourceOutput(
            compilationAndTypes,
            (spc, source) =>
            {
                var outputTypes = new Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>(
                    SymbolEqualityComparer.Default
                );
                var (compilation, types) = source;
                foreach (var type in types)
                {
                    if (type is null) continue;
                    var symbol = CompilationHelpers.GetSymbol(compilation, type);
                    if (symbol is null) continue;
                    var havePolymorphicAttribute = GeneratorHelpers.HasPolymorphicAttribute(symbol);
                    if (!havePolymorphicAttribute) continue;

                    foreach (var inheritTypes in types)
                    {
                        if (inheritTypes is null) continue;
                        var inheritSymbol = CompilationHelpers.GetSymbol(compilation, inheritTypes);
                        if (inheritSymbol is null) continue;
                        if (!CompilationHelpers.IsTopOfHierarchy(inheritSymbol, types!, compilation)) continue;
                        if (!inheritSymbol.InheritsFrom(symbol)) continue;
                        AddSymbol(outputTypes, symbol, inheritSymbol);
                    }
                }

                foreach (var outputType in outputTypes)
                {
                    var outputString = GeneratorHelpers.CreateSourceOutput(outputType);
                    spc.AddSource(
                        $"{outputType.Key.Name}_DerivedType.g.cs",
                        SourceText.From(outputString, Encoding.UTF8)
                    );
                }
            }
        );
    }

    private void AddSymbol(
        Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> dict,
        INamedTypeSymbol baseSymbol,
        INamedTypeSymbol inheritSymbol
    )
    {
        if (dict.ContainsKey(baseSymbol.OriginalDefinition))
        {
            dict[baseSymbol.OriginalDefinition].Add(inheritSymbol);
            return;
        }
        dict.Add(baseSymbol.OriginalDefinition, [inheritSymbol]);
    }
}