using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JsonDerivedTypeGenerator;

internal static class CompilationHelpers
{
    public static IEnumerable<INamedTypeSymbol> GetLeafSymbols(ImmutableArray<INamedTypeSymbol> allTypes)
    {
        var hasChildren = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var t in allTypes)
            if (t.BaseType != null)
                hasChildren.Add(t.BaseType);

        return allTypes.Where(t => !hasChildren.Contains(t));
    }

    public static bool InheritsFrom(INamedTypeSymbol symbol, INamedTypeSymbol potentialBase)
    {
        if (potentialBase.TypeKind == TypeKind.Interface)
            return symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, potentialBase));

        var current = symbol.BaseType;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, potentialBase))
                return true;
            current = current.BaseType;
        }
        return false;
    }

    public static INamedTypeSymbol? GetSymbol(Compilation compilation, TypeDeclarationSyntax type)
    {
        var model = compilation.GetSemanticModel(type.SyntaxTree);
        return model.GetDeclaredSymbol(type) as INamedTypeSymbol;
    }

    internal static bool IsAvailable(this INamedTypeSymbol symbol) =>
        symbol.DeclaredAccessibility != Accessibility.Private;
}
