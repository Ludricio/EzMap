// See https://aka.ms/new-console-template for more information

using EzMap;
using MyNamespace;

Console.WriteLine("Hello, World!");

var foo = new Foo { Bar = "test", Baz = 42 };
var dto = foo.MapToFooDto();
Console.WriteLine($"Dto: _barModel={dto._barModel}, _bazEntity={dto._bazEntity}");

namespace MyNamespace
{
    public class Foo
    {
        public string Bar { get; set; } = "";
        public int Baz { get; set; }
    }
}

public record FooDto
{
    public string _barModel { get; set; } = "";
    public long _bazEntity { get; init; }
}

[Map<Foo, FooDto>]
public static partial class Test;