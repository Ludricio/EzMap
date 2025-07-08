using EzMap.Generators.Diagnostics;

namespace EzMap.Generators.Pipelines
{
    internal interface IPipelineData
    {
        DiagnosticReport DiagnosticReport { get; }
    }
}