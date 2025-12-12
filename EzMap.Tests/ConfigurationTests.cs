using Microsoft.CodeAnalysis;

namespace EzMap.Tests;

public class ConfigurationTests
{
    [Fact]
    public void MappingDirection_SourceToTarget_GeneratesOnlyOneMethod()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source { public string Name { get; set; } = """"; }
    public class Target { public string Name { get; set; } = """"; }

    [Map<Source, Target>(MappingDirection = MappingDirection.SourceToTarget)]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("MapToTarget", generatedCode);
        Assert.DoesNotContain("MapToSource", generatedCode);
    }

    [Fact]
    public void MappingDirection_TargetToSource_GeneratesOnlyReverseMethod()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source { public string Name { get; set; } = """"; }
    public class Target { public string Name { get; set; } = """"; }

    [Map<Source, Target>(MappingDirection = MappingDirection.TargetToSource)]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.DoesNotContain("MapToTarget", generatedCode);
        Assert.Contains("MapToSource", generatedCode);
    }

    [Fact]
    public void MappingHooks_Enabled_GeneratesPartialMethods()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source { public string Name { get; set; } = """"; }
    public class Target { public string Name { get; set; } = """"; }

    [Map<Source, Target>(GenerateMappingHooks = true)]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("BeforeMap", generatedCode);
        Assert.Contains("AfterMap", generatedCode);
        Assert.Contains("static partial", generatedCode);
    }

    [Fact]
    public void CustomPrefixes_RemovedDuringMatching()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source { public string Name { get; set; } = """"; }
    public class Target { public string m_Name { get; set; } = """"; }

    [Map<Source, Target>(CustomPrefixes = new[] { ""m_"" })]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("m_Name = source.Name", generatedCode);
    }

    [Fact]
    public void CustomSuffixes_RemovedDuringMatching()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source { public string Name { get; set; } = """"; }
    public class Target { public string NameResponse { get; set; } = """"; }

    [Map<Source, Target>(CustomSuffixes = new[] { ""Response"" })]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("NameResponse = source.Name", generatedCode);
    }

    [Fact]
    public void GlobalConfiguration_AppliedToAllMappings()
    {
        var source = @"
using EzMap;

[assembly: MapperConfiguration(CustomPrefixes = new[] { ""m_"" })]

namespace TestNamespace
{
    public class Source { public string Name { get; set; } = """"; }
    public class Target { public string m_Name { get; set; } = """"; }

    [Map<Source, Target>]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("m_Name = source.Name", generatedCode);
    }
}
