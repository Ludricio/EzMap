using Microsoft.CodeAnalysis;

namespace EzMap.Tests;

public class NullabilityTests
{
    [Fact]
    public void NullableToNonNullable_UsesDefault()
    {
        var source = @"
#nullable enable
using EzMap;

namespace TestNamespace
{
    public class Source { public string? Name { get; set; } }
    public class Target { public string Name { get; set; } = """"; }

    [Map<Source, Target>]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("??", generatedCode);
        Assert.Contains("default", generatedCode);
    }

    [Fact(Skip = "Throw behavior implementation pending")]
    public void NullableToNonNullable_ThrowBehavior_GeneratesThrow()
    {
        var source = @"
#nullable enable
using EzMap;

namespace TestNamespace
{
    public class Source { public string? Name { get; set; } }
    public class Target { public string Name { get; set; } = """"; }

    [Map<Source, Target>(NullableFallbackBehavior = NullableFallbackBehavior.Throw)]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        // Verify throw pattern is present
        Assert.Contains("throw", generatedCode);
    }

    [Fact]
    public void NullableValueType_HandledCorrectly()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source { public int? Age { get; set; } }
    public class Target { public int Age { get; set; } }

    [Map<Source, Target>]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("??", generatedCode);
    }

    [Fact]
    public void NonNullableToNullable_DirectAssignment()
    {
        var source = @"
#nullable enable
using EzMap;

namespace TestNamespace
{
    public class Source { public string Name { get; set; } = """"; }
    public class Target { public string? Name { get; set; } }

    [Map<Source, Target>]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("Name = source.Name", generatedCode);
    }
}
