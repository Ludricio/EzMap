// See https://aka.ms/new-console-template for more information

using EzMap;
using MyNamespace;

Console.WriteLine("Hello, World!");

new Foo().ToDto();

namespace MyNamespace
{
    public class Foo
    {
        public string Bar { get; set; }
        public int Baz { get; set; }
    }
}

public record FooDto
{
    public Guid _barModel { get; set; }
    public required long _bazEntity { get; init; }
}

[
    MapClass<Foo, FooDto>
]
public static partial class Test;