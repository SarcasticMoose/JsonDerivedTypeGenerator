# JsonDerivedTypeGenerator

[![Stable](https://badgen.net/nuget/v/JsonDerivedTypeGenerator/v?color=blue&label=Stable)](https://www.nuget.org/packages/JsonDerivedTypeGenerator)

**JsonDerivedTypeGenerator** is a C# **source generator** that automatically adds `[JsonDerivedType]` attributes to base classes marked with `[JsonPolymorphic]`, enabling correct polymorphic serialization with `System.Text.Json`.

The generator supports:

- classes and interfaces
- public and internal access modifier
- deep inheritance

## Installation

You can add the generator to your project via NuGet:

```bash
  dotnet add package JsonDerivedTypeGenerator
````

> The generator runs at compile time — you don’t need to invoke it manually.

## Usage

1. Mark your base class as `[JsonPolymorphic]` and `partial`:

```csharp
using System.Text.Json.Serialization;

[JsonPolymorphic]
public abstract partial class Animal
{
    public abstract void MakeNoise();
    public abstract string Kind { get; }
}
```

> [!IMPORTANT]  
> Without marking your base class as `[JsonPolymorphic]` and `partial`, it will not work.


2. Implement your base class:

```csharp
public class Dog : Animal
{
    public override void MakeNoise() => {}
    public override string Kind => "Dog";
}

public class Cat : Animal
{
    public override void MakeNoise() => {}
    public override string Kind => "Cat";
}
```

3. When you build the project, the generator will automatically add `[JsonDerivedType]` attributes to the base class:

```csharp
[JsonDerivedType(typeof(Dog), "Dog")]
[JsonDerivedType(typeof(Cat), "Cat")]
public abstract partial class Animal
{
    public abstract void MakeNoise();
    public abstract string Kind { get; }
}
```
