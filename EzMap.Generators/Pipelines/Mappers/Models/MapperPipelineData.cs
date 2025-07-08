using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using EzMap.Generators.Diagnostics;
using EzMap.Generators.Pipelines.Mappers.Models.MappingInfos;

namespace EzMap.Generators.Pipelines.Mappers.Models
{
    internal sealed class MapperPipelineData : IPipelineData
    {
        public MapperInfo? MapperInfo { get; set; }
        public DiagnosticReport DiagnosticReport { get; } = new();

        [MemberNotNullWhen(true, nameof(MapperInfo))]
        public bool HasErrors => DiagnosticReport
            .Diagnostics
            .Count(d => d.Descriptor.DefaultSeverity is DiagnosticSeverity.Error) > 0;
    }
}