using Microsoft.CodeAnalysis;
using System;

namespace EzMap.Generators.Models;

/// <summary>
/// Internal model that includes type symbols for code generation.
/// This is not equatable and is only used within the generation stage.
/// </summary>
internal class MapperGenerationModel
{
    public MapperGenerationModel(
        MapperConfiguration configuration,
        ITypeSymbol sourceTypeSymbol,
        ITypeSymbol targetTypeSymbol,
        INamedTypeSymbol classSymbol)
    {
        Configuration = configuration;
        SourceTypeSymbol = sourceTypeSymbol;
        TargetTypeSymbol = targetTypeSymbol;
        ClassSymbol = classSymbol;
    }

    public MapperConfiguration Configuration { get; }
    public ITypeSymbol SourceTypeSymbol { get; }
    public ITypeSymbol TargetTypeSymbol { get; }
    public INamedTypeSymbol ClassSymbol { get; }
}
