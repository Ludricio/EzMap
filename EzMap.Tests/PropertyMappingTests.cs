using Microsoft.CodeAnalysis;

namespace EzMap.Tests;

public class PropertyMappingTests
{
    [Fact]
    public void UnderscorePrefix_AutomaticallyNormalized()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source { public string Name { get; set; } = """"; }
    public class Target { public string _Name { get; set; } = """"; }

    [Map<Source, Target>]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("_Name = source.Name", generatedCode);
    }

    [Fact]
    public void DtoSuffix_AutomaticallyNormalized()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source { public string Name { get; set; } = """"; }
    public class Target { public string NameDto { get; set; } = """"; }

    [Map<Source, Target>]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("NameDto = source.Name", generatedCode);
    }

    [Fact]
    public void ExplicitPropertyMapping_OverridesAutomatic()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source 
    { 
        public string Name1 { get; set; } = """";
        public string Name2 { get; set; } = """";
    }
    
    public class Target 
    { 
        public string TargetName1 { get; set; } = """";
        public string TargetName2 { get; set; } = """";
    }

    [Map<Source, Target>]
    [MapProperty(""Name1"", ""TargetName1"")]
    [MapProperty(""Name2"", ""TargetName2"")]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("TargetName1 = source.Name1", generatedCode);
        Assert.Contains("TargetName2 = source.Name2", generatedCode);
    }

    [Fact]
    public void TypeConversion_IntToLong_Works()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source { public int Value { get; set; } }
    public class Target { public long Value { get; set; } }

    [Map<Source, Target>]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("Value = source.Value", generatedCode);
    }

    [Fact]
    public void TypeConversion_LongToInt_UsesExplicitCast()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source { public long Value { get; set; } }
    public class Target { public int Value { get; set; } }

    [Map<Source, Target>]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("(int)", generatedCode);
    }

    [Fact]
    public void InitOnlyProperty_CanBeSet()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source { public string Name { get; set; } = """"; }
    public class Target { public string Name { get; init; } = """"; }

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
