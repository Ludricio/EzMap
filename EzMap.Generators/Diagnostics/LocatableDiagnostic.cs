using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Diagnostics
{
    internal class LocatableDiagnostic
    {
        public required DiagnosticDescriptor Descriptor { get; init; }
        public required Location Location { get; init; }
        public object[]? DescriptorArguments { get; init; }
    }
}