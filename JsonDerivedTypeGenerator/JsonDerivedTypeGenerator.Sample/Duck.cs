using System;

namespace JsonDerivedTypeGenerator.Sample;

public class Duck : Animal
{
    public override void MakeNoise()
    {
        Console.WriteLine("Quack");
    }

    public override string Kind { get; } = "Duck";
}