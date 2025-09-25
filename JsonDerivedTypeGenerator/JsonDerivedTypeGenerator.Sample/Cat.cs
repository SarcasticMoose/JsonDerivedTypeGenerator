using System;

namespace JsonDerivedTypeGenerator.Sample;

public class Cat : Animal
{
    public override void MakeNoise()
    {
        Console.WriteLine("Meow");
    }

    public override string Kind { get; } = "Cat";
}