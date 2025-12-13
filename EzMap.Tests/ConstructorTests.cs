using Microsoft.CodeAnalysis;

namespace EzMap.Tests;

public class ConstructorTests
{
    [Fact]
    public void ConstructorStrategy_Parameterless_Works()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source
    {
        public string Name { get; set; } = """";
    }

    public class Target
    {
        public string Name { get; set; } = """";
        
        public Target() { }
        public Target(string name) { Name = name; }
    }

    [Map<Source, Target>(ConstructorSelectionStrategy = ConstructorSelectionStrategy.Parameterless)]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("new global::TestNamespace.Target", generatedCode);
    }

    [Fact]
    public void ConstructorStrategy_Greediest_SelectsLargest()
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
        
        public Target() { }
        public Target(string name) { Name = name; }
        public Target(string name, int age) { Name = name; Age = age; }
    }

    [Map<Source, Target>(ConstructorSelectionStrategy = ConstructorSelectionStrategy.Greediest)]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        // Code should compile successfully with greediest constructor
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
    }

    [Fact]
    public void ConstructorStrategy_Annotated_SelectsMarkedConstructor()
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
        
        public Target() { }
        
        [MapConstructor]
        public Target(string name) { Name = name; }
        
        public Target(string name, int age) { Name = name; Age = age; }
    }

    [Map<Source, Target>(ConstructorSelectionStrategy = ConstructorSelectionStrategy.Annotated)]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
    }

    [Fact]
    public void NoAccessibleConstructor_EmitsDiagnostic()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Source
    {
        public string Name { get; set; } = """";
    }

    public class Target
    {
        public string Name { get; set; } = """";
        
        private Target() { }
    }

    [Map<Source, Target>]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Id == "EZMAP003");
    }

    [Fact]
    public void Record_WorksWithConstructor()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public record Source(string Name, int Age);
    public record Target(string Name, int Age);

    [Map<Source, Target>]
    public static partial class TestMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "TestMappers_Source_Target.g.cs");
        Assert.NotNull(generatedCode);
    }
}
