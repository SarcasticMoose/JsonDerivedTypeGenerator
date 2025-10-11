using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace JsonDerivedTypeGenerator.Tests;

public class DerivedTypesGeneratorStub
{
    private readonly string _vectorClassText;

    public DerivedTypesGeneratorStub(string vectorClassText)
    {
        _vectorClassText = vectorClassText;
    }
    
    public ImmutableArray<SyntaxTree> RunGenerator()
    {
        var generator = new DerivedTypesGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var compilation = CSharpCompilation.Create(nameof(FileGerationTests),
            new[] { CSharpSyntaxTree.ParseText(_vectorClassText) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(System.Text.Json.Serialization.JsonPolymorphicAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

        var runResult = driver.RunGenerators(compilation).GetRunResult();
        return runResult.GeneratedTrees;
    }
}