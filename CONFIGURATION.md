# EzMap Configuration Guide

This guide explains how to use the comprehensive configuration system in EzMap.

## Table of Contents
- [Global Configuration](#global-configuration)
- [Per-Mapping Configuration](#per-mapping-configuration)
- [Configuration Options](#configuration-options)
- [Examples](#examples)

## Global Configuration

Configure default behavior for all mappings in your assembly using the `MapperConfigurationAttribute`:

```csharp
[assembly: MapperConfiguration(
    CustomPrefixes = new[] { "_", "m_" },
    CustomSuffixes = new[] { "Dto", "Entity", "Model", "ViewModel" },
    EnablePrefixSuffixNormalization = true,
    CaseInsensitiveMatching = true,
    NullableFallbackBehavior = NullableFallbackBehavior.Default,
    AllowRecursiveMapping = false,
    MaxRecursionDepth = 5,
    ConstructorSelectionStrategy = ConstructorSelectionStrategy.Parameterless
)]
```

## Per-Mapping Configuration

Override global settings for specific mappings using the `Map<TSource, TTarget>` attribute:

```csharp
[Map<Foo, FooDto>(
    CustomPrefixes = new[] { "_" },
    EnablePrefixSuffixNormalization = true,
    CaseInsensitiveMatching = false,
    MappingDirection = MappingDirection.Both,
    GenerateMappingHooks = true,
    GenerateInstanceExtensions = true,
    GenerateStaticExtensions = false
)]
public static partial class MyMappers;
```

Alternatively, use `MapOptionsAttribute` for more complex scenarios:

```csharp
[Map<Foo, FooDto>]
[MapOptions(
    ForMapping = new[] { typeof(Foo), typeof(FooDto) },
    NullableFallbackBehavior = NullableFallbackBehavior.Throw,
    MappingDirection = MappingDirection.SourceToTarget
)]
public static partial class MyMappers;
```

## Configuration Options

### Name Normalization

**CustomPrefixes** - Prefixes to remove from property names during matching
- Type: `string[]`
- Default: `["_"]`
- Example: `_name` → `name`

**CustomSuffixes** - Suffixes to remove from property names during matching
- Type: `string[]`
- Default: `["Dto", "Entity", "Model", "ViewModel"]`
- Example: `NameDto` → `Name`

**EnablePrefixSuffixNormalization** - Enable/disable prefix/suffix removal
- Type: `bool`
- Default: `true`

**CaseInsensitiveMatching** - Use case-insensitive property name matching
- Type: `bool`
- Default: `true`
- When `true`: `Name` matches `name`, `NAME`, etc.

### Nullability Handling

**NullableFallbackBehavior** - How to handle nullable-to-non-nullable conversions
- Type: `NullableFallbackBehavior` enum
- Values:
  - `Default` - Use `default(T)` (default behavior)
  - `Throw` - Throw `ArgumentNullException` if source is null
  - `Diagnostic` - Emit warning diagnostic only
- Default: `Default`

Example generated code for `Throw`:
```csharp
Name = source.Name ?? throw new ArgumentNullException(nameof(source.Name))
```

### Recursive Mapping

**AllowRecursiveMapping** - Enable recursive mapping for complex types
- Type: `bool`
- Default: `false`
- When enabled, properties with custom types will use their own mappers

**MaxRecursionDepth** - Maximum depth for recursive mappings
- Type: `int`
- Default: `5`
- Prevents infinite recursion

### Constructor Selection

**ConstructorSelectionStrategy** - How to select constructor for target type
- Type: `ConstructorSelectionStrategy` enum
- Values:
  - `Parameterless` - Use parameterless constructor (default)
  - `Greediest` - Use constructor with most parameters
  - `Annotated` - Use constructor marked with `[MapConstructor]`
- Default: `Parameterless`

Example for `Annotated` strategy:
```csharp
public class FooDto
{
    [MapConstructor]
    public FooDto(string name, int age)
    {
        Name = name;
        Age = age;
    }
    
    public string Name { get; set; }
    public int Age { get; set; }
}
```

### Mapping Direction

**MappingDirection** - Control which mappings to generate
- Type: `MappingDirection` enum
- Values:
  - `Both` - Generate both `MapToTarget` and `MapToSource` (default)
  - `SourceToTarget` - Generate only `MapToTarget`
  - `TargetToSource` - Generate only `MapToSource`
- Default: `Both`

### Mapping Hooks

**GenerateMappingHooks** - Generate BeforeMap/AfterMap partial methods
- Type: `bool`
- Default: `false`

When enabled, generates:
```csharp
public static FooDto MapToFooDto(this Foo source)
{
    source = BeforeMap(source);
    
    var target = new FooDto { /* properties */ };
    
    target = AfterMap(source, target);
    return target;
}

static partial Foo BeforeMap(Foo source);
static partial FooDto AfterMap(Foo source, FooDto target);
```

Implement the partial methods in your class:
```csharp
public static partial class MyMappers
{
    static partial Foo BeforeMap(Foo source)
    {
        // Pre-processing logic
        return source;
    }
    
    static partial FooDto AfterMap(Foo source, FooDto target)
    {
        // Post-processing logic
        return target;
    }
}
```

### Extension Method Generation

**GenerateInstanceExtensions** - Generate `source.MapToTarget()` methods
- Type: `bool`
- Default: `true`

**GenerateStaticExtensions** - Generate static extension members (future C# feature)
- Type: `bool`
- Default: `false`

## Explicit Property Mapping

Use `MapPropertyAttribute` to explicitly map properties with different names:

```csharp
[Map<Foo, FooDto>]
[MapProperty("InternalName", "PublicName")]
[MapProperty("_id", "Id")]
public static partial class MyMappers;
```

Properties with explicit mappings are matched first, before automatic name matching.

## Examples

### Example 1: Strict Mapping with Throws

```csharp
[assembly: MapperConfiguration(
    EnablePrefixSuffixNormalization = false,
    CaseInsensitiveMatching = false,
    NullableFallbackBehavior = NullableFallbackBehavior.Throw
)]

[Map<User, UserDto>]
public static partial class UserMappers;
```

### Example 2: Flexible Mapping with Custom Conventions

```csharp
[Map<DomainModel, ApiModel>(
    CustomPrefixes = new[] { "m_", "_" },
    CustomSuffixes = new[] { "Model", "Dto" },
    CaseInsensitiveMatching = true
)]
public static partial class DomainMappers;
```

### Example 3: One-Way Mapping with Hooks

```csharp
[Map<CreateRequest, Entity>(
    MappingDirection = MappingDirection.SourceToTarget,
    GenerateMappingHooks = true
)]
public static partial class RequestMappers
{
    static partial Entity AfterMap(CreateRequest source, Entity target)
    {
        target.CreatedAt = DateTime.UtcNow;
        target.Id = Guid.NewGuid();
        return target;
    }
}
```

### Example 4: Complex Mapping with Explicit Properties

```csharp
[Map<Order, OrderDto>]
[MapProperty("OrderId", "Id")]
[MapProperty("CustomerInfo.Name", "CustomerName")] // Nested property
[MapOptions(
    NullableFallbackBehavior = NullableFallbackBehavior.Diagnostic,
    AllowRecursiveMapping = true
)]
public static partial class OrderMappers;
```

### Example 5: Using Annotated Constructor

```csharp
public class ImmutableDto
{
    [MapConstructor]
    public ImmutableDto(string name, int value)
    {
        Name = name;
        Value = value;
    }
    
    public string Name { get; }
    public int Value { get; }
}

[Map<Mutable, ImmutableDto>(
    ConstructorSelectionStrategy = ConstructorSelectionStrategy.Annotated
)]
public static partial class ImmutableMappers;
```

## Migration from Old Syntax

Old syntax:
```csharp
[MapClass<Foo, FooDto>]
public static partial class MyMappers;
```

New syntax (backward compatible):
```csharp
[Map<Foo, FooDto>]
public static partial class MyMappers;
```

The new syntax provides all the same functionality plus extensive configuration options.
