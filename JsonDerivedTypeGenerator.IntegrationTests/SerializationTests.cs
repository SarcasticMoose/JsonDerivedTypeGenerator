using System;
using System.Text.Json;
using Xunit;

namespace JsonDerivedTypeGenerator.IntegrationTests;

public class SerializationTests
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = false };

    [Fact]
    public void Serialize_DerivedType_IncludesTypeDiscriminator()
    {
        Animal animal = new Dog { Breed = "Labrador" };

        var json = JsonSerializer.Serialize(animal, Options);

        Assert.Contains("$type", json);
        Assert.Contains("Dog", json);
    }

    [Fact]
    public void Deserialize_WithTypeDiscriminator_ReturnsDerivedType()
    {
        Animal animal = new Dog { Breed = "Labrador" };
        var json = JsonSerializer.Serialize(animal, Options);

        var result = JsonSerializer.Deserialize<Animal>(json, Options);

        Assert.IsType<Dog>(result);
        Assert.Equal("Labrador", ((Dog)result!).Breed);
    }

    [Fact]
    public void Deserialize_MultipleTypes_ReturnsCorrectDerivedTypes()
    {
        var animals = new Animal[] { new Dog { Breed = "Poodle" }, new Cat { IsIndoor = true } };
        var json = JsonSerializer.Serialize(animals, Options);

        var result = JsonSerializer.Deserialize<Animal[]>(json, Options)!;

        Assert.IsType<Dog>(result[0]);
        Assert.IsType<Cat>(result[1]);
        Assert.Equal("Poodle", ((Dog)result[0]).Breed);
        Assert.True(((Cat)result[1]).IsIndoor);
    }

    [Fact]
    public void Serialize_IgnoredType_ThrowsNotSupportedException()
    {
        Animal animal = new UnknownAnimal();

        Assert.Throws<NotSupportedException>(() => JsonSerializer.Serialize(animal, Options));
    }

    [Fact]
    public void Serialize_DifferentPolymorphicHierarchies_WorkIndependently()
    {
        Animal animal = new Cat { IsIndoor = false };
        Shape shape = new Circle { Radius = 5 };

        var animalJson = JsonSerializer.Serialize(animal, Options);
        var shapeJson = JsonSerializer.Serialize(shape, Options);

        Assert.IsType<Cat>(JsonSerializer.Deserialize<Animal>(animalJson, Options));
        var circle = JsonSerializer.Deserialize<Shape>(shapeJson, Options) as Circle;
        Assert.NotNull(circle);
        Assert.Equal(5, circle.Radius);
    }

    [Fact]
    public void RoundTrip_PreservesAllProperties()
    {
        Animal original = new Dog { Breed = "Husky" };
        var json = JsonSerializer.Serialize(original, Options);

        var result = JsonSerializer.Deserialize<Animal>(json, Options) as Dog;

        Assert.NotNull(result);
        Assert.Equal("Husky", result.Breed);
        Assert.Equal("Dog", result.Kind);
    }
}
