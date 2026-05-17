# JsonDerivedTypeGenerator

[![Stable](https://img.shields.io/nuget/v/JsonDerivedTypeGenerator.svg?color=blue&label=Stable)](https://www.nuget.org/packages/JsonDerivedTypeGenerator)

**JsonDerivedTypeGenerator** is a C# **source generator** that automatically adds `[JsonDerivedType]` attributes to base classes marked with `[JsonPolymorphic]`, enabling correct polymorphic serialization with `System.Text.Json`.

The generator supports:

- classes and interfaces
- public and internal access modifier
- deep inheritance
- generic base classes and interfaces
- excluding specific derived types via `[JsonDerivedTypeIgnore]`

## Installation

> [!NOTE]
> `JsonDerivedTypeGenerator` depends on `JsonDerivedTypeGenerator.Attributes`, which is installed automatically as a transitive dependency.



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

## Generic base classes

Generic classes and interfaces are fully supported as polymorphic bases:

```csharp
[JsonPolymorphic]
public abstract partial class Result<T> { }

public class IntSuccess : Result<int> { }
public class StringSuccess : Result<string> { }
```

The generator will emit:

```csharp
[JsonDerivedType(typeof(IntSuccess), nameof(IntSuccess))]
[JsonDerivedType(typeof(StringSuccess), nameof(StringSuccess))]
public abstract partial class Result<T> { }
```

Multiple type parameters are also supported (`Either<TLeft, TRight>`, etc.).


## Excluding derived types

Mark a derived type with `[JsonDerivedTypeIgnore]` to exclude it from the generated attributes:

```csharp
using System.Text.Json.Serialization;

[JsonDerivedTypeIgnore]
public class UnknownAnimal : Animal
{
    public override void MakeNoise() => {}
    public override string Kind => "Unknown";
}
```

The generator will skip `UnknownAnimal` — no `[JsonDerivedType(typeof(UnknownAnimal), ...)]` will be emitted. Attempting to serialize an instance of an ignored type as the base type will throw `NotSupportedException` at runtime.
