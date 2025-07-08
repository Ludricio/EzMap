using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines
{
    internal interface IPipeline<out TData> where TData : IPipelineData
    {
        bool Filter(SyntaxNode syntaxNode, CancellationToken ct); 
        TData Transform(GeneratorAttributeSyntaxContext context, CancellationToken ct);
    }
}