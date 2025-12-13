using Microsoft.CodeAnalysis;

namespace EzMap.Tests;

public class BasicMappingTests
{
    [Fact]
    public void SimpleBidirectionalMapping_Generates()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source
    {
        public string Name { get; set; } = """";
        public int Age { get; set; }
    }

    public class Target
    {
        public string Name { get; set; } = """";
        public int Age { get; set; }
    }

    [Map<Source, Target>]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        // Should have no errors
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        // Should generate a file
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("MapToTarget", generatedCode);
        Assert.Contains("MapToSource", generatedCode);
    }

    [Fact]
    public void NonStaticClass_DoesNotGenerate()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source { public string Name { get; set; } = """"; }
    public class Target { public string Name { get; set; } = """"; }

    [Map<Source, Target>]
    public partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        // Should not generate mapping code
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.Null(generatedCode);
    }

    [Fact]
    public void MultipleProperties_AllMapped()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source
    {
        public string Text { get; set; } = """";
        public int Number { get; set; }
        public bool Flag { get; set; }
    }

    public class Target
    {
        public string Text { get; set; } = """";
        public int Number { get; set; }
        public bool Flag { get; set; }
    }

    [Map<Source, Target>]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("Text = source.Text", generatedCode);
        Assert.Contains("Number = source.Number", generatedCode);
        Assert.Contains("Flag = source.Flag", generatedCode);
    }

    [Fact]
    public void GlobalNamespace_Works()
    {
        var source = @"
using EzMap;

public class Foo
{
    public string Bar { get; set; } = """";
}

public class FooDto
{
    public string Bar { get; set; } = """";
}

[Map<Foo, FooDto>]
public static partial class Mappers { }
";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "Mappers_Foo_FooDto.g.cs");
        Assert.NotNull(generatedCode);
    }
}
