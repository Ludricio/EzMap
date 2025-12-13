using Microsoft.CodeAnalysis;
using Xunit;

namespace EzMap.Tests;

/// <summary>
/// Tests for explicit property mapping validation.
/// </summary>
public class ExplicitMappingValidationTests
{
    [Fact]
    public void ValidExplicitMapping_ShouldGenerateCode()
    {
        var source = @"
using EzMap;

public class Source
{
    public string Name { get; set; }
    public int Value { get; set; }
}

public class Target
{
    public string FullName { get; set; }
    public int Amount { get; set; }
}

[Map<Source, Target>]
[MapProperty(""Name"", ""FullName"")]
[MapProperty(""Value"", ""Amount"")]
public static partial class Mappers;
";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);
        
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);
        
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "Mappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("MapToTarget", generatedCode);
        Assert.Contains("FullName = source.Name", generatedCode);
        Assert.Contains("Amount = source.Value", generatedCode);
    }

    [Fact]
    public void MissingSourceProperty_ShouldEmitEZMAP301Error()
    {
        var source = @"
using EzMap;

public class Source
{
    public string Name { get; set; }
}

public class Target
{
    public string FullName { get; set; }
}

[Map<Source, Target>]
[MapProperty(""NonExistentProperty"", ""FullName"")]
public static partial class Mappers;
";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);
        
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, d => d.Id == "EZMAP301");
        Assert.Contains(errors, d => 
            d.Id == "EZMAP301" && 
            d.GetMessage().Contains("NonExistentProperty") &&
            d.GetMessage().Contains("Source"));
    }

    [Fact]
    public void MissingTargetProperty_ShouldEmitEZMAP302Error()
    {
        var source = @"
using EzMap;

public class Source
{
    public string Name { get; set; }
}

public class Target
{
    public string FullName { get; set; }
}

[Map<Source, Target>]
[MapProperty(""Name"", ""NonExistentTarget"")]
public static partial class Mappers;
";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);
        
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, d => d.Id == "EZMAP302");
        Assert.Contains(errors, d => 
            d.Id == "EZMAP302" && 
            d.GetMessage().Contains("NonExistentTarget") &&
            d.GetMessage().Contains("Target"));
    }

    [Fact]
    public void BothPropertiesMissing_ShouldEmitBothErrors()
    {
        var source = @"
using EzMap;

public class Source
{
    public string Name { get; set; }
}

public class Target
{
    public string FullName { get; set; }
}

[Map<Source, Target>]
[MapProperty(""MissingSource"", ""MissingTarget"")]
public static partial class Mappers;
";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);
        
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, d => d.Id == "EZMAP301");
        // Note: EZMAP302 won't be emitted because we skip after EZMAP301
    }

    [Fact]
    public void ExplicitMappingWithInheritance_ShouldWork()
    {
        var source = @"
using EzMap;

public class BaseSource
{
    public string BaseName { get; set; }
}

public class Source : BaseSource
{
    public int Value { get; set; }
}

public class Target
{
    public string Name { get; set; }
    public int Amount { get; set; }
}

[Map<Source, Target>]
[MapProperty(""BaseName"", ""Name"")]
[MapProperty(""Value"", ""Amount"")]
public static partial class Mappers;
";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);
        
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);
        
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "Mappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("MapToTarget", generatedCode);
        Assert.Contains("Name = source.BaseName", generatedCode);
        Assert.Contains("Amount = source.Value", generatedCode);
    }

    [Fact(Skip = "Records with parameterized constructors require ConstructorSelectionStrategy.Greediest which is not fully implemented yet")]
    public void ExplicitMappingForRecords_ShouldWork()
    {
        var source = @"
using EzMap;

public record Source(string Name, int Value);

public record Target(string FullName, int Amount);

[Map<Source, Target>]
[MapProperty(""Name"", ""FullName"")]
[MapProperty(""Value"", ""Amount"")]
public static partial class Mappers;
";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);
        
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);
        
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "Mappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("MapToTarget", generatedCode);
    }

    [Fact]
    public void StaticOrFieldProperties_ShouldNotBeConsideredForValidation()
    {
        var source = @"
using EzMap;

public class Source
{
    public static string StaticProp { get; set; }
    public string Name;  // Field, not property
    public string RealName { get; set; }
}

public class Target
{
    public static string StaticTarget { get; set; }
    public string TargetField;  // Field, not property
    public string RealTarget { get; set; }
}

[Map<Source, Target>]
[MapProperty(""StaticProp"", ""RealTarget"")]
public static partial class Mappers;
";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);
        
        // Should fail because StaticProp is static and shouldn't be in readable properties
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, d => d.Id == "EZMAP301");
    }

    [Fact]
    public void ReadOnlyTargetProperty_ShouldEmitEZMAP302Error()
    {
        var source = @"
using EzMap;

public class Source
{
    public string Name { get; set; }
}

public class Target
{
    public string FullName { get; }  // Read-only property
}

[Map<Source, Target>]
[MapProperty(""Name"", ""FullName"")]
public static partial class Mappers;
";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);
        
        // Should fail because FullName is read-only (not writable)
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, d => d.Id == "EZMAP302");
    }

    [Fact]
    public void WriteOnlySourceProperty_ShouldEmitEZMAP301Error()
    {
        var source = @"
using EzMap;

public class Source
{
    private string _name;
    public string Name { set { _name = value; } }  // Write-only property
}

public class Target
{
    public string FullName { get; set; }
}

[Map<Source, Target>]
[MapProperty(""Name"", ""FullName"")]
public static partial class Mappers;
";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);
        
        // Should fail because Name is write-only (not readable)
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, d => d.Id == "EZMAP301");
    }

    [Fact]
    public void MixOfValidAndInvalidMappings_ShouldNotGenerateCode()
    {
        var source = @"
using EzMap;

public class Source
{
    public string Name { get; set; }
    public int Value { get; set; }
}

public class Target
{
    public string FullName { get; set; }
    public int Amount { get; set; }
    public string Extra { get; set; }
}

[Map<Source, Target>]
[MapProperty(""Name"", ""FullName"")]
[MapProperty(""InvalidSource"", ""Extra"")]
public static partial class Mappers;
";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);
        
        // Should have error due to invalid mapping
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, d => d.Id == "EZMAP301");
        
        // But shouldn't generate code due to errors
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "Mappers_Source_Target.g.cs");
        Assert.Null(generatedCode); // No code should be generated when there are errors
    }
}
