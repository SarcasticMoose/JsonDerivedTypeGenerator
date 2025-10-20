using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JsonDerivedTypeGenerator;

internal static class CompilationHelpers
{
    public static bool IsTopOfHierarchy(
        INamedTypeSymbol typeSymbol,
        ImmutableArray<TypeDeclarationSyntax> types,
        Compilation compilation)
    {
        foreach (var symbol in types.Select(type => GetSymbol(compilation, type)))
        {
            if (symbol?.BaseType == null) continue;
            if (SymbolEqualityComparer.Default.Equals(symbol.BaseType, typeSymbol)) return false;
        }

        return true;
    }

    public static INamedTypeSymbol? GetSymbol(
        Compilation compilation, 
        TypeDeclarationSyntax type)
    {
        var model = compilation.GetSemanticModel(type.SyntaxTree);
        var symbol = model.GetDeclaredSymbol(type) as INamedTypeSymbol;
        return symbol;
    }

    public static string GetModifiers(
        this INamedTypeSymbol symbol)
    {
        List<string> parts =
        [
            symbol.DeclaredAccessibility switch
            {
                Accessibility.Public => "public",
                Accessibility.Internal => "internal",
                _ => "",
            },
        ];
        if (symbol is { TypeKind: TypeKind.Class, IsAbstract: true }) parts.Add("abstract");
        return string.Join(" ", parts.Where(p => !string.IsNullOrEmpty(p)));
    }

    public static bool InheritsFrom(this INamedTypeSymbol symbol, INamedTypeSymbol baseType)
    {
        if (baseType.TypeKind is TypeKind.Interface)
        {
            return symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, baseType));
        }
        
        var current = symbol.BaseType;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType)) return true;
            current = current.BaseType;
        }
        return false;
    }
}
