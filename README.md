# JsonDerivedTypeGenerator

[![Stable](https://badgen.net/nuget/v/jsonderivedtypegenerator/v?color=blue&label=Stable)](https://www.nuget.org/packages/JsonDerivedTypeGenerator)

**JsonDerivedTypeGenerator** is a C# **source generator** that automatically adds `[JsonDerivedType]` attributes to base classes marked with `[JsonPolymorphic]`, enabling correct polymorphic serialization with `System.Text.Json`.

The generator supports:

- Abstract classes (`abstract`) and interfaces (`interface`).  
- Generating derived type attributes only for classes marked with `[JsonSerializable]`.  
- Adding `[JsonDerivedType]` attributes **directly on the base type**, not in separate wrapper classes.  

---

## Installation

You can add the generator to your project via NuGet:

```bash
dotnet add package JsonDerivedTypeGenerator
````

> The generator runs at compile time — you don’t need to invoke it manually.

---

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

2. Make sure your derived classes are public and optionally marked `[JsonSerializable]`:

```csharp
[JsonSerializable]
public class Dog : Animal
{
    public override void MakeNoise() => {}
    public override string Kind => "Dog";
}

[JsonSerializable]
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

---

## Notes

* The base class **must be partial** so the generator can safely append attributes.
* The generator only generates attributes for all implementation of base class that have `[JsonPolimorphic]`
* This generator works only for classes (for now)

---