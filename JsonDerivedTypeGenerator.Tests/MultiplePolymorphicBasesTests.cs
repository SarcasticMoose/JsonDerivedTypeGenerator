using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace JsonDerivedTypeGenerator.Tests;

public class MultiplePolymorphicBasesTests
{
    private static readonly Regex JsonDerivedTypePattern =
        new(@"(?<!\w)\[JsonDerivedType\s*\(\s*typeof\s*\(\s*([\w\.\<\>\?,\s]+)\s*\)\s*,\s*nameof\s*\(\s*([\w\.\+]+)\s*\)\s*\)\s*\]",
            RegexOptions.Compiled);

    [Fact]
    public void Generate_TwoPolymorphicBases_EachLeafOnlyAddedToItsOwnBase()
    {
        // Arrange — Cat:Animal, Dog:Animal, Eagle:Bird (separate hierarchy)
        const string source = """
            using System.Text.Json.Serialization;

            namespace Sample;

            [JsonPolymorphic]
            public abstract partial class Animal { }

            [JsonPolymorphic]
            public abstract partial class Bird { }

            public class Cat : Animal { }
            public class Dog : Animal { }
            public class Eagle : Bird { }
            """;

        var stub = new DerivedTypesGeneratorStub(source);
        var trees = stub.RunGenerator(nameof(MultiplePolymorphicBasesTests));
        var combined = string.Join("\n", trees.Select(t => t.GetText()));

        var animalFile = trees.First(t => t.FilePath.EndsWith("Animal_DerivedType.g.cs")).ToString();
        var birdFile = trees.First(t => t.FilePath.EndsWith("Bird_DerivedType.g.cs")).ToString();

        // Animal gets Cat and Dog — NOT Eagle
        Assert.Equal(2, JsonDerivedTypePattern.Matches(animalFile).Count);
        Assert.Contains("Cat", animalFile);
        Assert.Contains("Dog", animalFile);
        Assert.DoesNotContain("Eagle", animalFile);

        // Bird gets Eagle — NOT Cat or Dog
        Assert.Single(JsonDerivedTypePattern.Matches(birdFile));
        Assert.Contains("Eagle", birdFile);
        Assert.DoesNotContain("Cat", birdFile);
        Assert.DoesNotContain("Dog", birdFile);
    }

    [Fact]
    public void Generate_TwoPolymorphicBases_GeneratesTwoFiles()
    {
        const string source = """
            using System.Text.Json.Serialization;

            namespace Sample;

            [JsonPolymorphic]
            public abstract partial class Vehicle { }

            [JsonPolymorphic]
            public abstract partial class Animal { }

            public class Car : Vehicle { }
            public class Cat : Animal { }
            """;

        var stub = new DerivedTypesGeneratorStub(source);
        var trees = stub.RunGenerator(nameof(MultiplePolymorphicBasesTests));

        Assert.Equal(2, trees.Length);
    }

    [Fact]
    public void Generate_PolymorphicInterfaceAndClass_NoCrossContamination()
    {
        const string source = """
            using System.Text.Json.Serialization;

            namespace Sample;

            [JsonPolymorphic]
            public partial interface IShape { }

            [JsonPolymorphic]
            public abstract partial class Animal { }

            public class Circle : IShape { }
            public class Cat : Animal { }
            """;

        var stub = new DerivedTypesGeneratorStub(source);
        var trees = stub.RunGenerator(nameof(MultiplePolymorphicBasesTests));

        var shapeFile = trees.First(t => t.FilePath.EndsWith("IShape_DerivedType.g.cs")).ToString();
        var animalFile = trees.First(t => t.FilePath.EndsWith("Animal_DerivedType.g.cs")).ToString();

        Assert.Contains("Circle", shapeFile);
        Assert.DoesNotContain("Cat", shapeFile);

        Assert.Contains("Cat", animalFile);
        Assert.DoesNotContain("Circle", animalFile);
    }

    [Fact]
    public void Generate_ClassInheritsFromTwoPolymorphicBases_ViaInterfaceAndClass()
    {
        // FlyingCat : Animal + IFlyable — should appear in BOTH
        const string source = """
            using System.Text.Json.Serialization;

            namespace Sample;

            [JsonPolymorphic]
            public abstract partial class Animal { }

            [JsonPolymorphic]
            public partial interface IFlyable { }

            public class FlyingCat : Animal, IFlyable { }
            """;

        var stub = new DerivedTypesGeneratorStub(source);
        var trees = stub.RunGenerator(nameof(MultiplePolymorphicBasesTests));

        var animalFile = trees.First(t => t.FilePath.EndsWith("Animal_DerivedType.g.cs")).ToString();
        var flyableFile = trees.First(t => t.FilePath.EndsWith("IFlyable_DerivedType.g.cs")).ToString();

        Assert.Contains("FlyingCat", animalFile);
        Assert.Contains("FlyingCat", flyableFile);
    }

    [Fact]
    public void Generate_DeepInheritanceWithMultipleBases_LeafOnlyAddedToAncestors()
    {
        const string source = """
            using System.Text.Json.Serialization;

            namespace Sample;

            [JsonPolymorphic]
            public abstract partial class Animal { }

            [JsonPolymorphic]
            public abstract partial class FlyingAnimal : Animal { }

            public class Eagle : FlyingAnimal { }
            """;

        var stub = new DerivedTypesGeneratorStub(source);
        var trees = stub.RunGenerator(nameof(MultiplePolymorphicBasesTests));

        // Eagle is leaf → added to both Animal (ancestor) and FlyingAnimal (direct base)
        var animalFile = trees.First(t => t.FilePath.EndsWith("Animal_DerivedType.g.cs")).ToString();
        var flyingFile = trees.First(t => t.FilePath.EndsWith("FlyingAnimal_DerivedType.g.cs")).ToString();

        Assert.Contains("Eagle", animalFile);
        Assert.Contains("Eagle", flyingFile);
    }

    [Fact]
    public void Generate_UnrelatedLeaf_NotAddedToAnyBase()
    {
        const string source = """
            using System.Text.Json.Serialization;

            namespace Sample;

            [JsonPolymorphic]
            public abstract partial class Animal { }

            public class Car { }
            public class Cat : Animal { }
            """;

        var stub = new DerivedTypesGeneratorStub(source);
        var trees = stub.RunGenerator(nameof(MultiplePolymorphicBasesTests));

        Assert.Single(trees);
        var animalFile = trees.First().ToString();
        Assert.DoesNotContain("Car", animalFile);
        Assert.Contains("Cat", animalFile);
    }
}
