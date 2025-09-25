using System.Text.Json.Serialization;

namespace JsonDerivedTypeGenerator.Sample;

[JsonPolymorphic]
public abstract partial class Animal
{
    public abstract void MakeNoise();
    public abstract string Kind { get; }

    public virtual string Debug => "Debug";
}