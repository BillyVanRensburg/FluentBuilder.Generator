# FluentBuilder Source Generator – Complete User Guide

## Introduction
The FluentBuilder source generator automatically creates a fluent builder class for any C# class or record decorated with the `[FluentBuilder]` attribute. The generated builder provides a type-safe, expressive way to construct instances of your types, with support for validation, async operations, collections, nested builders, and extensive customisation.

This document covers every feature of the generator, from basic usage to advanced scenarios. After reading this guide, you will be able to:

- Quickly generate a fluent builder for any type.
- Customise builder names, method prefixes, namespaces, and accessibility.
- Add validation rules (sync/async) using attributes.
- Work with collections (lists, dictionaries) and configure collection helper methods.
- Handle asynchronous methods and async builders.
- Integrate custom factory methods.
- Use implicit conversions and truth operators.
- Understand diagnostic messages and troubleshoot common issues.

## Installation
Add the NuGet package to your project:

```bash
dotnet add package FluentBuilder.Generator
```

Or via Package Manager:

```powershell
Install-Package FluentBuilder.Generator
```

The generator works with any project targeting .NET Standard 2.0 or later (including .NET Core, .NET 5/6/7/8/9/10 and .NET Framework). It requires the `Microsoft.CodeAnalysis.CSharp` package, which is automatically referenced.

## Basic Usage
### For Classes
1. Decorate your class with `[FluentBuilder]`:

```csharp
using FluentBuilder;

[FluentBuilder]
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

2. The generator creates a `PersonBuilder` class in the same namespace. Use it like this:

```csharp
var person = new PersonBuilder()
    .WithName("John Doe")
    .WithAge(30)
    .Build();

Console.WriteLine(person.Name); John Doe
```

### For Records
Records work seamlessly:

```csharp
[FluentBuilder]
public record Person(string Name, int Age);
```

## Configuration via MSBuild
You can set global defaults in your project file (`.csproj`):

```xml
<PropertyGroup>
  <FluentBuilder_DefaultBuilderSuffix>Builder</FluentBuilder_DefaultBuilderSuffix>
  <FluentBuilder_DefaultBuilderNamespace></FluentBuilder_DefaultBuilderNamespace>
  <FluentBuilder_DefaultBuilderAccessibility>public</FluentBuilder_DefaultBuilderAccessibility>
  <FluentBuilder_DefaultMethodPrefix>With</FluentBuilder_DefaultMethodPrefix>
  <FluentBuilder_DefaultMethodSuffix></FluentBuilder_DefaultMethodSuffix>
  <FluentBuilder_DefaultGeneratePartial>false</FluentBuilder_DefaultGeneratePartial>
  <FluentBuilder_EnableLogging>false</FluentBuilder_EnableLogging>
</PropertyGroup>
```

These settings provide fallback values when not overridden by attributes.

## Attributes Reference
The generator recognises a rich set of attributes that control every aspect of builder generation.

### Core Builder Attributes
#### `[FluentBuilder]`
Marks a class or record for builder generation. Required for the generator to process the type.

**Properties** *(all optional)*:

- `GeneratePartial` – if `true`, generates a partial class (default `false`).
- `BuilderAccessibility` (Enum) – sets the accessibility of the builder class. Default `Public`. The value is specified as an enum member of `BuilderAccessibility`. The generator maps the enum value to the corresponding C# keyword.

| Enum Value          | C# Keyword          | Valid Context                                      |
|---------------------|---------------------|----------------------------------------------------|
| Public              | public              | Top‑level and nested types                         |
| Internal            | internal            | Top‑level and nested types                         |
| Private             | private             | Nested types only                                  |
| Protected           | protected           | Nested types only                                  |
| ProtectedInternal   | protected internal  | Nested types only                                  |
| PrivateProtected    | private protected   | Nested types only (C# 7.2+)                        |
| File                | file                | **Not supported** for `BuilderAccessibility` (see note) |

> **Important notes:**
> - For **top‑level** types (not nested), only `Public`, `Internal`, and (since C# 11) `File` are permitted. Specifying any other value on a top‑level type results in a diagnostic error (FBBLD0001 from the analyzer, or FB028 during generation).
> - For **nested types**, all values are allowed, subject to standard C# accessibility rules (e.g., a nested class cannot be more accessible than its containing type).
> - The `File` accessibility (C# 11+) is **not supported** for `BuilderAccessibility` because the generated builder class is placed in a separate source file (`.g.cs`). With `file` accessibility, the builder would be confined to its own generated file and inaccessible from the original file where the attribute is applied.

- `BuilderNamespace` – overrides the namespace of the generated builder (default is same as target type).
- `MethodPrefix` – a string to prepend to every fluent method name (e.g., `"With"`). Default `"With"`.
- `MethodSuffix` – a string to append to every fluent method name (e.g., `"Async"`). Default empty.

**Example:**

```csharp
[FluentBuilder(GeneratePartial = true, MethodPrefix = "With")]
public class Order { ... }
```

#### `[FluentName]`
Specifies a custom name for the generated builder class (when applied to the class) or for a specific fluent method (when applied to a property, field, or method).

**Constructor**: `FluentNameAttribute(string name)`

**Usage:**

```csharp
[FluentName("PersonBuilder")] renames the builder class
public class Person
{
    [FluentName("SetAge")] renames the fluent method for Age
    public int Age { get; set; }
}
```

#### `[FluentIgnore]`
Instructs the generator to skip a member (property, field, method, constructor). No fluent method will be created for it.

```csharp
public class Person
{
    [FluentIgnore]
    public string InternalId { get; set; }
}
```

#### `[FluentInclude]`
Explicitly includes a non‑public member (internal) in the builder. By default, only public instance members are included.

```csharp
public class Person
{
    [FluentInclude]
    internal string Nickname { get; set; }
}
```

#### `[FluentDefaultValue]`
Sets a default value for a property or field. The builder will initialise the member with this value unless it is explicitly set via a fluent method.

**Constructor**: `FluentDefaultValueAttribute(object value)`

```csharp
public class Person
{
    [FluentDefaultValue(30)]
    public int Age { get; set; }
}
```

#### `[FluentBuilderBuildMethod]`
Changes the name of the synchronous build method (default `"Build"`).

**Constructor**: `FluentBuilderBuildMethodAttribute(string methodName = "Build")`

```csharp
[FluentBuilder]
[FluentBuilderBuildMethod("Create")]
public class Order { ... }
Now builder.Create() returns an Order.
```

#### `[FluentBuilderFactoryMethod]`
Specifies a static factory method to use instead of a constructor. The method must be `public`, `static`, parameterless, and return the target type.

**Constructor**: `FluentBuilderFactoryMethodAttribute(string methodName)`

```csharp
[FluentBuilder]
[FluentBuilderFactoryMethod("Create")]
public class MyClass
{
    public static MyClass Create() => new MyClass();
}
```

#### `[FluentImplicit]`
Enables implicit conversion operators from the builder to the target type (and optionally to `Task<T>` if async support is enabled).

```csharp
[FluentBuilder]
[FluentImplicit]
public class Person { ... }

Usage:
Person p = new PersonBuilder(); implicit conversion calls Build()
```

If async support is enabled, an implicit conversion to `Task<Person>` is also generated, calling `BuildAsync()`.

#### `[FluentTruthOperator]`
Adds the `true`, `false`, and `!` operators to the builder. The operators return `true` only after `Build()` has been called.

```csharp
[FluentBuilder]
[FluentTruthOperator]
public class MyClass { ... }

Usage:
var builder = new MyClassBuilder();
if (builder) { /* Build not called → false */ }
builder.Build();
if (builder) { /* Now true */ }
```

### Collection Support
#### `[FluentCollectionOptions]`
Applied to a collection property or field to enable additional fluent methods and validation.

**Properties**:

- `GenerateDirectSetter` (`bool`) – generate the direct setter (e.g., `Items(value)`). Default `true`.
- `GenerateActionSetter` (`bool`) – generate an action‑based setter (e.g., `Items(action)`). Default `true`.
- `GenerateAdd` (`bool`) – generate `Add{MemberName}(item)` methods. Default `false`.
- `GenerateRemove` (`bool`) – generate `Remove{MemberName}(item)` methods. Default `false`.
- `GenerateClear` (`bool`) – generate `Clear{MemberName}()` method. Default `false`.
- `GenerateAddRange` (`bool`) – generate `Add{MemberName}Range(items)` (for `List<T>`). Default `false`.
- `GenerateCount` (`bool`) – generate a `{MemberName}Count` property and count validation. Default `false`.
- `MinCount` (`int`) – minimum required items (if `GenerateCount` is true). Default -1 (no minimum).
- `MaxCount` (`int`) – maximum allowed items. Default -1 (no maximum).
- `ExactCount` (`int`) – exact required count (overrides min/max). Default -1.
- `CountValidationMessage` (`string?`) – custom validation message for count checks.

**Example:**

```csharp
public class Order
{
    [FluentCollectionOptions(GenerateAdd = true, GenerateRemove = true, GenerateCount = true, MinCount = 1)]
    public List<string> Tags { get; set; }
}
```

This generates:
- `AddTags(string item)`
- `RemoveTags(string item)`
- `TagsCount` property (int)
- In `Build()`, validates that `Tags.Count >= 1`.

### Validation Attributes
The generator supports both synchronous and asynchronous validation. Validation methods are automatically called during `Build()` (or `BuildAsync()`) and throw an exception if validation fails.

#### `[FluentValidate]` (base attribute)
Can be applied to properties or fields. It provides common validation checks via named arguments.

**Named arguments:**

- `MinLength` (`int`) – for strings.
- `MaxLength` (`int`) – for strings.
- `RegexPattern` (`string?`) – for strings.
- `MinValue` (`double`) – for numeric types.
- `MaxValue` (`double`) – for numeric types.
- `Required` (`bool`) – value cannot be `null`.
- `CustomMessage` (`string?`) – override default error message.

**Example:**

```csharp
public class Person
{
    [FluentValidate(Required = true, MinLength = 2, MaxLength = 50)]
    public string Name { get; set; }

    [FluentValidate(MinValue = 0, MaxValue = 120)]
    public int Age { get; set; }
}
```

#### Specialised Validation Attributes
These are derived from `FluentValidateAttribute` and provide preconfigured validators:

- `[FluentValidateEmail]` – validates a string as an email address (simple regex).
- `[FluentValidatePhone]` – validates a phone number (basic international format).
- `[FluentValidateUrl]` – validates a URL.
- `[FluentValidateRange(int min, int max)]` – validates a numeric range.
- `[FluentValidateRange(double min, double max)]` – double range.
- `[FluentValidateEqual(object value)]` – must equal the given value.
- `[FluentValidateNotEqual(object value)]` – must not equal the given value.
- `[FluentValidateGreaterThan(object value)]` – must be greater than value.
- `[FluentValidateGreaterThanOrEqual(object value)]` – must be greater or equal than value.
- `[FluentValidateLessThan(object value)]` – must be less than value.
- `[FluentValidateLessThanOrEqual(object value)]` – must be less or equal than value.
- `[FluentValidateOneOf(params object[] values)]` – must be one of the provided values.

**Examples:**

```csharp
[FluentValidateEmail]
public string Email { get; set; }

[FluentValidateRange(18, 65)]
public int Age { get; set; }

[FluentValidateOneOf("Red", "Green", "Blue")]
public string Color { get; set; }
```

#### `[FluentValidateWith]`
Allows you to specify a custom synchronous validator class.

**Constructor**: `FluentValidateWithAttribute(Type validatorType)`

**Named arguments:**

- `MethodName` (`string`) – name of the validation method on the validator (default `"Validate"`). The method must be public, instance, take the member's type, and return `bool`.
- `CustomMessage` (`string?`) – custom error message.

**Example:**

```csharp
public class AgeValidator
{
    public bool Validate(int age) => age >= 18;
}

public class Person
{
    [FluentValidateWith(typeof(AgeValidator))]
    public int Age { get; set; }
}
```

The generator creates a `ValidateAge()` method that instantiates the validator and calls `Validate(age)`.

#### `[FluentValidateAsync]`
Specifies an asynchronous validator.

**Constructor**: `FluentValidateAsyncAttribute(Type validatorType, string methodName)`

- `validatorType` – a type with a public parameterless constructor.
- `methodName` – name of a method that takes the member's value and returns `Task<bool>`. The method may optionally accept a `CancellationToken`.

**Example:**

```csharp
public class EmailValidator
{
    public async Task<bool> IsValidAsync(string email, CancellationToken token = default)
    {
        ... async validation logic
    }
}

public class Person
{
    [FluentValidateAsync(typeof(EmailValidator), nameof(EmailValidator.IsValidAsync))]
    public string Email { get; set; }
}
```

The generator creates a `ValidateEmailAsync()` method that awaits the validator.

#### `[FluentValidationMethod]`
Marks a method as a custom validation method to be automatically called during `Build()` or `BuildAsync()`.

The method must be:

- `public`, instance, parameterless.
- Return type can be `void`, `bool`, `Task`, or `Task<bool>`.
- If returning `bool` or `Task<bool>`, a `false` result causes an exception.
- In synchronous builds, only sync methods (`void`/`bool`) are called. In async builds, both sync and async methods are called.

**Example:**

```csharp
[FluentBuilder]
public class Order
{
    public List<Item> Items { get; set; }

    [FluentValidationMethod]
    public bool ValidateTotal() => Items.Sum(i => i.Price) > 0;

    [FluentValidationMethod]
    public async Task<bool> CheckInventoryAsync()
    {
        ...
    }
}
```

### Async Support
Async support can be enabled by adding `[FluentBuilderAsyncSupport]` to the class, or automatically if the class contains async methods or async validators.

#### `[FluentBuilderAsyncSupport]`
Enables async features and provides configuration.

**Properties** *(all optional)*:

- `AsyncBuildMethodName` (`string`) – name of the async build method (default `"BuildAsync"`).
- `AsyncValidationPrefix` (`string`) – prefix for async validation methods (default `"Validate"`).
- `GenerateAsyncBuild` (`bool`) – whether to generate the async build method (default `true`).
- `GenerateAsyncValidation` (`bool`) – whether to generate async validation methods (default `true`).
- `GenerateCancellationTokens` (`bool`) – whether async methods include a `CancellationToken` parameter (default `true`).

**Example:**

```csharp
[FluentBuilder]
[FluentBuilderAsyncSupport(AsyncBuildMethodName = "CreateAsync", GenerateCancellationTokens = true)]
public class MyService
{
    public async Task InitializeAsync(CancellationToken token = default) { ... }
}
```

When async support is enabled, the generator produces:
- An `async` version of every fluent method that corresponds to an async method in the target class (using `[FluentAsyncMethod]` or by removing the `"Async"` suffix).
- An async build method (`BuildAsync()` by default) that awaits any async methods and async validators.
- Extension methods on `Task<Builder>` to allow chaining on awaited tasks.

#### `[FluentAsyncMethod]`
Applied to an async method in the target class to control the name of the synchronous-looking fluent method that calls it.

**Property:**

- `SyncMethodName` (`string`) – the name of the generated fluent method (which internally awaits the async method). If not specified, the generator removes the `"Async"` suffix (if present).

**Example:**

```csharp
public class DataLoader
{
    [FluentAsyncMethod(SyncMethodName = "Load")]
    public async Task LoadAsync() { ... }
}
```

This generates a fluent method `Load()` that calls `LoadAsync()`.

## Builder Generation Process
When you apply `[FluentBuilder]`, the generator:

1. Scans all public instance properties and fields (unless `[FluentIgnore]` or not settable).
2. Scans all public instance methods (ordinary) for potential fluent inclusion.
3. Reads attributes on the type and members to customise names, validation, collection handling, etc.
4. Generates a builder class with:
   - Fluent setter methods for each included property/field.
   - Fluent methods for each included non‑async method.
   - Async fluent methods for each async method (if async support enabled).
   - Validation methods (sync and async) for each member with validation attributes.
   - A `Build()` method that creates the instance, applies values, runs validations, and returns it.
   - Optionally, an async `BuildAsync()` method.
   - Optionally, implicit conversion operators.
   - Optionally, truth operators.
   - Extension methods for async chaining.

## Nested Builders
If a property type itself has a `[FluentBuilder]` attribute, the generated builder will include a method that accepts an `Action<PropertyBuilder>` to configure the nested builder.

**Example:**

```csharp
[FluentBuilder]
public class Address
{
    public string Street { get; set; }
}

[FluentBuilder]
public class Person
{
    public Address HomeAddress { get; set; }
}

Usage:
var person = new PersonBuilder()
    .HomeAddress(home => home.WithStreet("123 Main St"))
    .Build();
```

The generator automatically detects cycles and reports an error (FB007) if a circular reference is found (e.g., `A` references `B`, which references `A`).

## Diagnostics and Logging
The generator emits diagnostics (errors, warnings, info) to help you correct issues. Diagnostic IDs range from **FB001** to **FB999**. See the Diagnostics Reference for a complete list.

To enable detailed logging (for debugging the generator itself), set:

```xml
<PropertyGroup>
  <FluentBuilder_EnableLogging>true</FluentBuilder_EnableLogging>
</PropertyGroup>
```

Logs are written to the Debug output (visible in Visual Studio's Output window when debugging).

### Generator Version Information
The generator automatically includes a **public** static class `GeneratorInfo` in the `FluentBuilder.Generated` namespace. This class provides information about the version of the FluentBuilder generator that was used to compile your project. You can use it in your code to display or log the generator version, for example:

```csharp
Console.WriteLine(GeneratorInfo.GetInfo()); Outputs: FluentBuilder.Generator v1.2.3.4
```

The `Version` constant reflects the exact version of the generator assembly at the time of compilation.

## Analyzers Rules

### General

### FBGEN001: Circular reference detected
**Severity:** Error  
**Category:** General  

**Description:**  
A circular dependency has been detected in the nested builder graph. For example, type `A` has a property of type `B`, and type `B` has a property of type `A` (directly or through a chain of other types). This would cause the generated builder code to recurse infinitely.

**Why this rule exists:**  
The FluentBuilder generator creates nested builder methods for properties whose types also have the `[FluentBuilder]` attribute. If a cycle exists, the generated code would attempt to nest builders endlessly, leading to a compiler error or stack overflow. This diagnostic catches the problem at design time.

**How to fix:**  
- Remove the circular reference by changing one of the properties to not use a nested builder (e.g., use a simple setter or a factory method).  
- Redesign your types to break the cycle, for example by using an ID reference instead of a direct object reference.

### FluentBuilder

#### FBBLD001: Invalid BuilderAccessibility for top-level type
**Severity:** Error  
**Category:** Usage  

**Description:**  
The `BuilderAccessibility` property of `FluentBuilderAttribute` can only be set to `Public` or `Internal` when the attributed type is a top‑level class or record.

**Why this rule exists:**  
The builder class generated by the FluentBuilder source generator is itself a top‑level type. Accessibility modifiers like `private` or `protected` are meaningless for a non‑nested type. By restricting allowed values, this analyzer prevents generation of uncompilable code.

**How to fix:**  
Change the `BuilderAccessibility` named argument to either `BuilderAccessibility.Public` or `BuilderAccessibility.Internal`. If you do not specify the argument at all, the default accessibility (`Public`) is used.

#### FBBLD002: BuilderAccessibility.File requires C# 11 or higher
**Severity:** Error  
**Category:** Usage  

**Description:**  
The `BuilderAccessibility.File` value can only be used when the project is compiled with C# 11 or a later version.

**Why this rule exists:**  
The `file` accessibility modifier was introduced in C# 11. Using it in an earlier language version would cause a compiler error.

**How to fix:**  
Upgrade your project to C# 11 or later (set `<LangVersion>11.0</LangVersion>` in your `.csproj`), or change the accessibility to `Public` or `Internal`.

#### FBBLD003: BuilderAccessibility.PrivateProtected requires C# 7.2 or higher
**Severity:** Error  
**Category:** Usage  

**Description:**  
The `BuilderAccessibility.PrivateProtected` value requires C# 7.2 or higher.

**Why this rule exists:**  
The `private protected` accessibility modifier was introduced in C# 7.2. Using it in an older version would cause a compiler error.

**How to fix:**  
Upgrade your project to C# 7.2 or later, or use a different accessibility value.

#### FBBLD004: Builder accessibility cannot exceed target type accessibility
**Severity:** Error  
**Category:** FluentBuilder  

**Description:**  
The generated builder class must not be more accessible than the type it builds.

**Why this rule exists:**  
In C#, a generated type cannot have greater accessibility than the type it builds. Violating this leads to compiler errors. This analyzer catches the inconsistency early.

**How to fix:**  
Reduce the `BuilderAccessibility` to match or be less permissive than the target type's accessibility. For example, if the target is `internal`, the builder cannot be `public`.

#### FBBLD005: Nested builder accessibility cannot exceed containing type accessibility
**Severity:** Error  
**Category:** FluentBuilder  

**Description:**  
For nested builders, the builder's accessibility must be at most the accessibility of its containing type.

**Why this rule exists:**  
A nested type cannot be more accessible than its container. This rule ensures the generated builder obeys C# accessibility rules.

**How to fix:**  
Set the `BuilderAccessibility` to a value that is not more permissive than the containing type's accessibility. For example, if the containing class is `internal`, the builder cannot be `public`.

#### FBBLD006: Builder name conflicts with existing type
**Severity:** Error  
**Category:** Naming  

**Description:**  
The generated builder class name (either the default `TypeName + "Builder"` or a custom name specified via `[FluentName]`) conflicts with an existing type in the same namespace or, for nested targets, within the same containing type.

**Why this rule exists:**  
If the generated builder’s name clashes with an already defined type, the compilation will fail with a CS0102 error (duplicate type name). This error is reported early so you can resolve the conflict before code generation.

**How to fix:**  
Use the `[FluentName]` attribute on your class to specify a different, unique name for the builder. For example, `[FluentName("CustomBuilder")]`. If a custom namespace is set via `BuilderNamespace`, ensure that the name is unique in that namespace.

#### FBBLD007: Builder accessibility incompatible with constructor accessibility
**Severity:** Warning  
**Category:** FluentBuilder  

**Description:**  
The builder accessibility specified on the `[FluentBuilder]` attribute is more permissive than any accessible constructor of the target type. In this situation the generated builder would not be able to instantiate the type.

**Why this rule exists:**  
The generated builder must be able to create instances of the target type. If the builder is more accessible than all of the type's constructors (for example the builder is `public` but all constructors are `private`), instantiation will fail at compile time or runtime. This analyzer catches the mismatch early.

**How to fix:**  
- Make at least one constructor of the target type accessible to the builder (e.g., add an `internal` or `public` constructor).  
- Or reduce the builder's accessibility via the `BuilderAccessibility` named argument on `[FluentBuilder]` so it is not more permissive than the available constructors.  
- Alternatively, provide a public static factory method and point the generator at it using `[FluentBuilderFactoryMethod("MethodName")]`; the generator can use that factory instead of calling a constructor.  
- Note: records with a primary constructor are assumed to have an accessible constructor and are exempt from this diagnostic.

#### FBBLD008: Cannot generate builder for abstract class
**Severity:** Error  
**Category:** FluentBuilder  

**Description:**  
The `[FluentBuilder]` attribute is applied to an abstract class or record. The FluentBuilder source generator cannot create a concrete builder for an abstract type because there is no way to instantiate it.

**Why this rule exists:**  
The generated builder must be able to construct instances of the target type. Abstract classes cannot be instantiated directly, so any attempt to generate a builder for them would result in non‑compilable code. This analyzer catches the misconfiguration early and prevents the generation of an unusable builder.

**How to fix:**  
- Remove the `[FluentBuilder]` attribute from the abstract class.  
- If you need a builder for a derived concrete class, apply the attribute to that concrete class instead.  
- Alternatively, consider using a factory pattern to create instances of concrete implementations and apply the attribute to those concrete types.

#### FBBLD009: Cannot generate builder for static class
**Severity:** Error  
**Category:** FluentBuilder  

**Description:**  
The `[FluentBuilder]` attribute is applied to a static class. Static classes cannot be instantiated, so a builder cannot be generated.

**Why this rule exists:**  
The generated builder must be able to create instances of the target type. Static classes are sealed and contain only static members; they cannot be instantiated. Applying the attribute would lead to non‑compilable code.

**How to fix:**  
- Remove the `[FluentBuilder]` attribute from the static class.  
- If you need a builder for a type that provides factory methods, consider using an ordinary (non‑static) class.

#### FBBLD010: No accessible constructor found
**Severity:** Warning  
**Category:** FluentBuilder  

**Description:**  
The type decorated with `[FluentBuilder]` has no constructor that is accessible to the generated builder (i.e., public, internal, or protected internal). Without an accessible constructor, the builder cannot instantiate the type.

**Why this rule exists:**  
The generated builder must be able to create instances of the target type. If all constructors are private, protected, or private protected, the builder code will fail to compile. This warning alerts you to the issue so you can provide an accessible constructor or switch to a factory method.

**How to fix:**  
- Add a public, internal, or protected internal constructor to the type.  
- Use a public static factory method by applying `[FluentBuilderFactoryMethod("MethodName")]` – the generator will then use that factory instead of a constructor.  
- If the type already has a public parameterless constructor (implicitly provided when no constructors are declared), no warning is shown.

#### FBBLD011: Nested builder type not found
**Severity:** Warning  
**Category:** FluentBuilder  

**Description:**  
A property in a type decorated with `[FluentBuilder]` has a type that is a user‑defined class or record, but that type does **not** have the `[FluentBuilder]` attribute. As a result, the generated builder will not include a nested builder method for this property; it will only generate a simple setter.

**Why this rule exists:**  
You may have intended to use a nested builder to configure the property’s object. This warning reminds you to add `[FluentBuilder]` to the property’s type if you want nested builder support. If the type intentionally does not need a builder, you can ignore this warning or suppress it.

**How to fix:**  
- Add the `[FluentBuilder]` attribute to the property’s type (if it is your code).  
- If the type is from an external library and you cannot add the attribute, consider wrapping it in your own builder type, or ignore the warning.  
- If you do not need nested builder support, no action is required.

#### FBBLD012: Missing required assembly reference
**Severity:** Error  
**Category:** FluentBuilder  

**Description:**  
The code contains one or more `FluentBuilder` attributes (e.g., `[FluentBuilder]`, `[FluentName]`, etc.), but the attribute types cannot be resolved. This usually means the project is missing a reference to the assembly that defines these attributes.

**Why this rule exists:**  
Without the attribute definitions, the FluentBuilder source generator cannot function, and the attributes themselves are treated as errors by the compiler. This diagnostic gives a clear, actionable error message directing the user to add the required NuGet package or assembly reference.

**How to fix:**  
- Add the `FluentBuilder.Generator` NuGet package to your project:  
  ```bash
  dotnet add package FluentBuilder.Generator
  ```
- If you are using a direct assembly reference, ensure it is correctly referenced.

## Conclusion
The FluentBuilder source generator provides a comprehensive, attribute‑driven way to create expressive, type‑safe builders for your C# types. With support for validation, async, collections, and deep customisation, it fits a wide range of scenarios.

For further assistance, please consult the source code repository or open an issue on GitHub.