using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace JsonDerivedTypeGenerator.Tests;

public class JsonDerivedTypeIgnoreTests
{
    private string JsonDerivedTypeRegexPattern =
        @"(?<!\w)\[JsonDerivedType\s*\(\s*typeof\s*\(\s*([\w\.\<\>\?,\s]+)\s*\)\s*,\s*nameof\s*\(\s*([\w\.\+]+)\s*\)\s*\)\s*\]";

    [Fact]
    public void Generate_ShouldSkipIgnoredDerivedType()
    {
        //Arrange
        string source =
            @"
        using System.Text.Json.Serialization;

        namespace JsonDerivedTypeGenerator.Sample;

        [JsonPolymorphic]
        public abstract partial class Animal
        {
            public abstract void MakeNoise();
        }

        public class Dog : Animal
        {
            public override void MakeNoise() { }
        }

        [JsonDerivedTypeIgnore]
        public class Cat : Animal
        {
            public override void MakeNoise() { }
        }
        ";
        var generator = new DerivedTypesGeneratorStub(source);

        //Act
        var generatedTree = generator.RunGenerator(nameof(JsonDerivedTypeIgnoreTests)).First();

        //Assert
        var text = generatedTree.ToString();
        var matches = Regex.Matches(text, JsonDerivedTypeRegexPattern);
        Assert.Single(matches);
        Assert.Contains("Dog", matches[0].Value);
        Assert.DoesNotContain("Cat", text);
    }

    [Fact]
    public void Generate_ShouldProduceNoFileWhenAllDerivedTypesIgnored()
    {
        //Arrange
        string source =
            @"
        using System.Text.Json.Serialization;

        namespace JsonDerivedTypeGenerator.Sample;

        [JsonPolymorphic]
        public abstract partial class Animal
        {
            public abstract void MakeNoise();
        }

        [JsonDerivedTypeIgnore]
        public class Dog : Animal
        {
            public override void MakeNoise() { }
        }

        [JsonDerivedTypeIgnore]
        public class Cat : Animal
        {
            public override void MakeNoise() { }
        }
        ";
        var generator = new DerivedTypesGeneratorStub(source);

        //Act
        var generatedTrees = generator.RunGenerator(nameof(JsonDerivedTypeIgnoreTests));

        //Assert
        Assert.Empty(generatedTrees);
    }
}
