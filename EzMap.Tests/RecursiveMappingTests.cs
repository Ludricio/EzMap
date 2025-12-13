using Microsoft.CodeAnalysis;

namespace EzMap.Tests;

public class RecursiveMappingTests
{
    [Fact]
    public void RecursiveMapping_Enabled_GeneratesMapperCall()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Customer
    {
        public string Name { get; set; } = """";
    }

    public class CustomerDto
    {
        public string Name { get; set; } = """";
    }

    public class Order
    {
        public int Id { get; set; }
        public Customer Customer { get; set; } = new();
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public CustomerDto Customer { get; set; } = new();
    }

    [Map<Customer, CustomerDto>]
    public static partial class CustomerMappers { }

    [Map<Order, OrderDto>(AllowRecursiveMapping = true)]
    public static partial class OrderMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "OrderMappers_Order_OrderDto.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("MapToCustomerDto()", generatedCode);
    }

    [Fact]
    public void RecursiveMapping_Disabled_UsesDefault()
    {
        var source = @"
using EzMap;

namespace TestNamespace
{
    public class Customer
    {
        public string Name { get; set; } = """";
    }

    public class CustomerDto
    {
        public string Name { get; set; } = """";
    }

    public class Order
    {
        public int Id { get; set; }
        public Customer Customer { get; set; } = new();
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public CustomerDto Customer { get; set; } = new();
    }

    [Map<Customer, CustomerDto>]
    public static partial class CustomerMappers { }

    [Map<Order, OrderDto>(AllowRecursiveMapping = false)]
    public static partial class OrderMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "OrderMappers_Order_OrderDto.g.cs");
        Assert.NotNull(generatedCode);
        Assert.DoesNotContain("MapToCustomerDto()", generatedCode);
        Assert.Contains("default(", generatedCode);
    }

    [Fact]
    public void RecursiveMapping_NullableProperty_HandlesNull()
    {
        var source = @"
#nullable enable
using EzMap;

namespace TestNamespace
{
    public class Address
    {
        public string Street { get; set; } = """";
    }

    public class AddressDto
    {
        public string Street { get; set; } = """";
    }

    public class Person
    {
        public string Name { get; set; } = """";
        public Address? Address { get; set; }
    }

    public class PersonDto
    {
        public string Name { get; set; } = """";
        public AddressDto? Address { get; set; }
    }

    [Map<Address, AddressDto>]
    public static partial class AddressMappers { }

    [Map<Person, PersonDto>(AllowRecursiveMapping = true)]
    public static partial class PersonMappers { }
}";

        var (compilation, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        var generatedCode = GeneratorTestHelper.GetGeneratedSource(compilation, "PersonMappers_Person_PersonDto.g.cs");
        Assert.NotNull(generatedCode);
        Assert.Contains("MapToAddressDto()", generatedCode);
        Assert.Contains("?.", generatedCode); // Null-conditional operator
    }
}
