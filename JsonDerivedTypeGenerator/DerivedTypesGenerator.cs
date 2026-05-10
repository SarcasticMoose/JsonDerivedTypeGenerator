using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
                var comparer = SymbolEqualityComparer.Default;
                var outputTypes = new Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>(comparer);
                var (compilation, types) = source;

                var allSymbols = types
                    .Where(t => t != null)
                    .Select(t => CompilationHelpers.GetSymbol(compilation, t!))
                    .Where(s => s != null)
                    .Select(s => s!)
                    .Where(s => s.IsAvailable())
                    .ToImmutableArray();

                var polymorphicSet = new HashSet<INamedTypeSymbol>(comparer);
                foreach (var s in allSymbols)
                    if (GeneratorHelpers.HasPolymorphicAttribute(s))
                        polymorphicSet.Add(s);

                var polymorphicSymbols = polymorphicSet.ToArray();

                var leafSymbols = CompilationHelpers
                    .GetLeafSymbols(allSymbols)
                    .Where(s => !polymorphicSet.Contains(s) && s.TypeKind != TypeKind.Interface)
                    .ToArray();

                foreach (var leafSymbol in leafSymbols)
                {
                    foreach (var polymorphicSymbol in polymorphicSymbols)
                    {
                        if (!CompilationHelpers.InheritsFrom(leafSymbol, polymorphicSymbol))
                            continue;

                        AddSymbol(outputTypes, polymorphicSymbol, leafSymbol);
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

    private static void AddSymbol(
        Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> dict,
        INamedTypeSymbol baseSymbol,
        INamedTypeSymbol leafSymbol
    )
    {
        var key = baseSymbol.OriginalDefinition;
        if (dict.TryGetValue(key, out var list))
            list.Add(leafSymbol);
        else
            dict[key] = [leafSymbol];
    }
}
