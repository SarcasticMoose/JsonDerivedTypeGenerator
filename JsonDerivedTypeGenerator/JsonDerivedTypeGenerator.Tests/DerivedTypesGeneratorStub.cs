using System.Collections.Immutable;
using System.Reflection;
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

    public ImmutableArray<SyntaxTree> RunGenerator(string assemblyName)
    {
        var generator = new DerivedTypesGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { CSharpSyntaxTree.ParseText(_vectorClassText) },
            new[]
            {
                MetadataReference.CreateFromFile(
                    typeof(System.Text.Json.Serialization.JsonPolymorphicAttribute)
                        .Assembly
                        .Location
                ),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
            }
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();
        return runResult.GeneratedTrees;
    }
}
