using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Diagnostics
{
    internal class DiagnosticReport
    {
        public List<LocatableDiagnostic> Diagnostics { get; } = [];

        public void Register(DiagnosticDescriptor descriptor, Location? location, params object[]? args)
        {
            Diagnostics.Add(new LocatableDiagnostic
            {
                Descriptor = descriptor, Location = location ?? Location.None, DescriptorArguments = args
            });
        }
    }
}