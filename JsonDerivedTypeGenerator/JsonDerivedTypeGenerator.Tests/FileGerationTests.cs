using System.Linq;
using Xunit;

namespace JsonDerivedTypeGenerator.Tests;

public class FileGerationTests
{
    [Fact]
    public void Generate_ShouldOnlyGenerateOneFile()
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
        var generatedTrees = generator.RunGenerator();

        //Assert
        Assert.Single(generatedTrees);
    }

    [Fact]
    public void Generate_ShouldHaveCorrectFileName()
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
        var generatedTrees = generator.RunGenerator();

        //Assert
        var generatedFileSyntax = generatedTrees.Single(t =>
            t.FilePath.EndsWith("Animal_DerivedType.g.cs")
        );
        Assert.NotNull(generatedFileSyntax);
    }

    [Fact]
    public void Generate_ShouldNotWorkWithInterface()
    {
        //Arrange
        string vectorClassText =
            @"
        using System.Text.Json.Serialization;

        namespace JsonDerivedTypeGenerator.Sample;

        [JsonPolymorphic]
        public partial interface IAnimal
        {
            public abstract void MakeNoise();
        }
        
        public class Cat : IAnimal
        {
            public override void MakeNoise()
            {
                Console.WriteLine(""Meow"");
            }
        }
        ";
        var generator = new DerivedTypesGeneratorStub(vectorClassText);

        //Act
        var generatedTrees = generator.RunGenerator();

        //Assert
        var generatedFileSyntax = generatedTrees.FirstOrDefault();
        Assert.NotNull(generatedFileSyntax);
    }

    [Fact]
    public void Generate_ShouldWorkWithDeepInheritance()
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

        public abstract partial class FlyingAnimal : Animal
        {
            public abstract void MakeNoise();
            public string Kind => /""Flying""
        }

        public class Bird : FlyingAnimal
        {
            public override void MakeNoise()
            {
                Console.WriteLine(""Tweet"");
            }
        }
        ";
        var generator = new DerivedTypesGeneratorStub(vectorClassText);

        //Act
        var generatedTrees = generator.RunGenerator();

        //Assert
        Assert.Single(generatedTrees);
    }
}
