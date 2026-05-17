using System;
using System.Text.Json.Serialization;

namespace JsonDerivedTypeGenerator.IntegrationTests;

[JsonPolymorphic]
public abstract partial class Animal
{
    public abstract string Kind { get; }
}

public class Dog : Animal
{
    public override string Kind => "Dog";
    public string Breed { get; set; } = string.Empty;
}

public class Cat : Animal
{
    public override string Kind => "Cat";
    public bool IsIndoor { get; set; }
}

[JsonDerivedTypeIgnore]
public class UnknownAnimal : Animal
{
    public override string Kind => "Unknown";
}

[JsonPolymorphic]
public abstract partial class Shape
{
    public abstract double Area();
}

public class Circle : Shape
{
    public double Radius { get; set; }
    public override double Area() => Math.PI * Radius * Radius;
}

public class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }
    public override double Area() => Width * Height;
}
