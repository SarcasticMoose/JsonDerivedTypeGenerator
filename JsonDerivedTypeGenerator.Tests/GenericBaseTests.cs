using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace JsonDerivedTypeGenerator.Tests;

public class GenericBaseTests
{
    private string JsonDerivedTypeRegexPattern =
        @"(?<!\w)\[JsonDerivedType\s*\(\s*typeof\s*\(\s*([\w\.\<\>\?,\s]+)\s*\)\s*,\s*nameof\s*\(\s*([\w\.\+]+)\s*\)\s*\)\s*\]";

    [Fact]
    public void Generate_ShouldHandleGenericBase()
    {
        //Arrange
        string source =
            @"
        using System.Text.Json.Serialization;

        namespace JsonDerivedTypeGenerator.Sample;

        [JsonPolymorphic]
        public abstract partial class Result<T>
        {
        }

        public class IntSuccess : Result<int>
        {
        }

        public class StringSuccess : Result<string>
        {
        }
        ";
        var generator = new DerivedTypesGeneratorStub(source);

        //Act
        var generatedTree = generator.RunGenerator(nameof(GenericBaseTests)).First();

        //Assert
        var text = generatedTree.ToString();
        Assert.Equal(2, Regex.Matches(text, JsonDerivedTypeRegexPattern).Count);
        Assert.Contains("partial class Result<T>", text);
    }

    [Fact]
    public void Generate_ShouldHandleGenericBaseWithMultipleTypeParams()
    {
        //Arrange
        string source =
            @"
        using System.Text.Json.Serialization;

        namespace JsonDerivedTypeGenerator.Sample;

        [JsonPolymorphic]
        public abstract partial class Either<TLeft, TRight>
        {
        }

        public class Left<TLeft, TRight> : Either<TLeft, TRight>
        {
        }

        public class Right<TLeft, TRight> : Either<TLeft, TRight>
        {
        }
        ";
        var generator = new DerivedTypesGeneratorStub(source);

        //Act
        var generatedTree = generator.RunGenerator(nameof(GenericBaseTests)).First();

        //Assert
        var text = generatedTree.ToString();
        Assert.Contains("partial class Either<TLeft, TRight>", text);
    }

    [Fact]
    public void Generate_ShouldHandleGenericInterface()
    {
        //Arrange
        string source =
            @"
        using System.Text.Json.Serialization;

        namespace JsonDerivedTypeGenerator.Sample;

        [JsonPolymorphic]
        public partial interface IResult<T>
        {
        }

        public class IntSuccess : IResult<int>
        {
        }

        public class StringSuccess : IResult<string>
        {
        }
        ";
        var generator = new DerivedTypesGeneratorStub(source);

        //Act
        var generatedTree = generator.RunGenerator(nameof(GenericBaseTests)).First();

        //Assert
        var text = generatedTree.ToString();
        Assert.Equal(2, Regex.Matches(text, JsonDerivedTypeRegexPattern).Count);
        Assert.Contains("partial interface IResult<T>", text);
    }
}
