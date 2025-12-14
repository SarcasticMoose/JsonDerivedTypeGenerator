using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                var outputTypes = new ConcurrentDictionary<INamedTypeSymbol, List<INamedTypeSymbol>>(comparer);
                var (compilation, types) = source;

                var allSymbols = types
                    .Where(t => t != null)
                    .Select(t => CompilationHelpers.GetSymbol(compilation, t!))
                    .Where(s => s != null)
                    .Select(s => s!)
                    .Where(s => s.IsAvailable())
                    .ToImmutableArray();

                var polymorphicSymbols = allSymbols
                    .Where(GeneratorHelpers.HasPolymorphicAttribute)
                    .ToArray();

                var topSymbols = allSymbols.Except(polymorphicSymbols).ToArray();
                
                foreach (var topSymbol in topSymbols)
                {
                    foreach (var polymorphicSymbol in polymorphicSymbols)
                    {
                        if (SymbolEqualityComparer.Default.Equals(polymorphicSymbol, topSymbol)) 
                            continue;

                        if (topSymbol.TypeKind == TypeKind.Interface) continue;
                        
                        if (!CompilationHelpers.IsTopOfHierarchy(topSymbol, allSymbols)) 
                            continue;
                        
                        AddSymbol(outputTypes, polymorphicSymbol, topSymbol);
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
        ConcurrentDictionary<INamedTypeSymbol, List<INamedTypeSymbol>> dict,
        INamedTypeSymbol baseSymbol,
        INamedTypeSymbol inheritSymbol
    )
    {
        if (dict.ContainsKey(baseSymbol.OriginalDefinition))
        {
            dict[baseSymbol.OriginalDefinition].Add(inheritSymbol);
            return;
        }
        dict.TryAdd(baseSymbol.OriginalDefinition, [inheritSymbol]);
    }
}