using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JsonDerivedTypeGenerator;

internal static class CompilationHelpers
{
    public static bool IsTopOfHierarchy(INamedTypeSymbol symbol, IEnumerable<INamedTypeSymbol> allTypes)
    {
        var hasDerived = allTypes.Any(t => t.BaseType != null && SymbolEqualityComparer.Default.Equals(t.BaseType, symbol));
        return !hasDerived;
    }
    
    public static INamedTypeSymbol? GetSymbol(
        Compilation compilation, 
        TypeDeclarationSyntax type)
    {
        var model = compilation.GetSemanticModel(type.SyntaxTree);
        var symbol = model.GetDeclaredSymbol(type) as INamedTypeSymbol;
        return symbol;
    }
    
    internal static bool IsAvailable(this INamedTypeSymbol symbol) => symbol.DeclaredAccessibility != Accessibility.Private;
}
