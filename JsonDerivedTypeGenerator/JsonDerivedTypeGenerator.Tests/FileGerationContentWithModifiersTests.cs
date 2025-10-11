using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace JsonDerivedTypeGenerator.Tests;

public class FileGerationContentWithModifiersTests
{
    [Theory]
    [InlineData("public")]
    [InlineData("internal")]
    public void Generate_ShouldWorkWithAccessModifiers(string accessModifier)
    {
        //Arrange
        string vectorClassText =
            @"
        using System.Text.Json.Serialization;

        namespace JsonDerivedTypeGenerator.Sample;

        [JsonPolymorphic]
        {@AccessModifier} partial class Animal
        {
            public abstract void MakeNoise();
            public abstract string Kind { get; }
        }
        public class Cat : Animal
        {
            public override void MakeNoise()
            {
                Console.WriteLine(""Meow"");
            }

            public override string Kind { get; }
        }
        ";

        var vectorClassTextWithModifiers = VectorClassFormatter.ReplaceTokens(
            vectorClassText,
            new Dictionary<string, string>() { ["AccessModifier"] = accessModifier }
        );
        var generator = new DerivedTypesGeneratorStub(vectorClassTextWithModifiers);

        //Act
        var generatedTrees = generator.RunGenerator().First();

        //Assert
        Assert.NotNull(generatedTrees);
    }

    [Fact]
    public void Generate_ShouldNotWorkWithPrivateModifier()
    {
        //Arrange
        string vectorClassText =
            @"
        using System.Text.Json.Serialization;

        namespace JsonDerivedTypeGenerator.Sample;

        [JsonPolymorphic]
        private partial class Animal
        {
            public abstract void MakeNoise();
            public abstract string Kind { get; }
        }
        public class Cat : Animal
        {
            public override void MakeNoise()
            {
                Console.WriteLine(""Meow"");
            }

            public override string Kind { get; }
        }
        ";

        var generator = new DerivedTypesGeneratorStub(vectorClassText);

        //Act
        var generatedTrees = generator.RunGenerator().FirstOrDefault();

        //Assert
        Assert.Null(generatedTrees);
    }

    [Fact]
    public void Generate_ShouldWorkWithAbstractModifier()
    {
        //Arrange
        string vectorClassText =
            @"
        using System.Text.Json.Serialization;

        namespace JsonDerivedTypeGenerator.Sample;

        [JsonPolymorphic]
        public abstract partial class Animal
        {
            public abstract void MakeNoise();
            public abstract string Kind { get; }
        }
        public class Cat : Animal
        {
            public override void MakeNoise()
            {
                Console.WriteLine(""Meow"");
            }

            public override string Kind { get; }
        }
        ";

        var generator = new DerivedTypesGeneratorStub(vectorClassText);

        //Act
        var generatedTrees = generator.RunGenerator().FirstOrDefault();

        //Assert
        Assert.NotNull(generatedTrees);
    }
}
