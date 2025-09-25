using System;

namespace JsonDerivedTypeGenerator.Sample;

public class Dog : Animal
{
    public override void MakeNoise()
    {
        Console.WriteLine("Howl");
    }

    public override string Kind { get; } = "Dog";
}