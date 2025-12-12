using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using EzMap.Generators;
using System.Reflection;
using System.Collections.Immutable;

namespace EzMap.Tests;

public class GeneratorTestHelper
{
    public static (Compilation compilation, ImmutableArray<Diagnostic> diagnostics) RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(MapAttribute<,>).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
        };

        // Add more required references
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
            {
                if (assembly.FullName?.StartsWith("System.") == true)
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }
        }

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new MapGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation, 
            out var outputCompilation, 
            out var diagnostics);

        return (outputCompilation, diagnostics);
    }

    public static string? GetGeneratedSource(Compilation compilation, string fileName)
    {
        var generatedTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains(fileName));
        
        return generatedTree?.ToString();
    }
}
