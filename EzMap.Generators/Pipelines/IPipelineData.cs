using EzMap.Generators.Diagnostics;
using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines
{
    internal interface IPipelineData
    {
        DiagnosticReport DiagnosticReport { get; }
    }
}