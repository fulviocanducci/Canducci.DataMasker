# Canducci.DataMasker

[![NuGet](https://img.shields.io/nuget/v/Canducci.DataMasker.svg)](https://www.nuget.org/packages/Canducci.DataMasker/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Canducci.DataMasker.svg)](https://www.nuget.org/packages/Canducci.DataMasker/)
[![Build and Tests](https://github.com/fulviocanducci/Canducci.DataMasker/actions/workflows/dotnet-tests.yml/badge.svg)](https://github.com/fulviocanducci/Canducci.DataMasker/actions/workflows/dotnet-tests.yml)

**Canducci.DataMasker** is a .NET library for masking sensitive information in objects through simple attributes. It helps protect personal, financial, and other confidential data when preparing objects for logs, diagnostics, reports, test output, and other scenarios where the complete original value should not be exposed.

The library does more than replace characters in a string: it can create a masked copy of an object graph, traverse nested objects and supported collections, and preserve reference relationships while leaving the source object unchanged.

**Documentation:** [Português (Brasil)](README.pt-BR.md)

## Contents

- [Highlights](#highlights)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quick start](#quick-start)
- [Mask types](#mask-types)
- [Mask strategies](#mask-strategies)
- [Nested objects and collections](#nested-objects-and-collections)
- [Custom masking](#custom-masking)
- [Invalid and empty values](#invalid-and-empty-values)
- [Object creation and custom factory](#object-creation-and-custom-factory)
- [Supported members and limitations](#supported-members-and-limitations)
- [Security considerations](#security-considerations)
- [Running the tests](#running-the-tests)
- [Contributing](#contributing)

## Highlights

- Attribute-based configuration with `[Mask]`.
- Ready-to-use rules for common personal, contact, financial, and sensitive data.
- Multiple masking strategies: full, type-aware partial, first characters, last characters, and custom.
- Recursive masking of nested objects.
- Support for one-dimensional arrays and `IList` collections, including `List<T>`.
- Cloning that keeps the original object and supported collections untouched.
- Preservation of repeated object references and circular references during cloning.
- Unicode-aware text handling, including combined characters and emoji.
- Fail-closed behavior for malformed, non-empty values handled by structured mask types.
- A custom factory option for controlling the root object's creation.

## Requirements

- .NET Standard 2.0 compatible library.
- The package can be consumed by applications targeting compatible .NET implementations, including modern .NET.

## Installation

Install the package from NuGet:

```bash
dotnet add package Canducci.DataMasker
```

Or add the package reference to your project:

```xml
<PackageReference Include="Canducci.DataMasker" Version="YOUR_VERSION" />
```

Replace `YOUR_VERSION` with the version you want to use. See the [Canducci.DataMasker package on NuGet](https://www.nuget.org/packages/Canducci.DataMasker/).

## Quick start

Import the library namespaces:

```csharp
using Canducci.DataMasker;
using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;
```

Decorate the properties that contain sensitive information:

```csharp
public class Customer
{
    [Mask(MaskType.Name)]
    public string Name { get; set; }

    [Mask(MaskType.Cpf)]
    public string Cpf { get; set; }

    [Mask(MaskType.Email)]
    public string Email { get; set; }

    [Mask(MaskType.Phone)]
    public string Phone { get; set; }
}
```

Create a masked copy:

```csharp
var customer = new Customer
{
    Name = "Fulvio Canducci",
    Cpf = "12345678901",
    Email = "fulvio@example.com",
    Phone = "18999991234"
};

var maskedCustomer = DataMasker.Mask(customer);
```

The call returns a new object with the configured properties masked. The original `customer` instance is not modified.

> Only properties decorated with `[Mask]` are masked. Other supported properties are copied as part of the cloned object graph.

## Mask types

The `MaskType` enum defines the built-in masking rules.

| Mask type | Behavior |
|---|---|
| `None` | Keeps the original value when used with the partial strategy. |
| `Custom` | Reserved for custom masking scenarios; use the custom strategy overload to invoke a custom method. |
| `Name` | Preserves the first text element of each word. |
| `Cpf` | Preserves the first three and last two digits. |
| `Cnpj` | Preserves the first two and last two digits. |
| `Rg` | Preserves the last three text elements. |
| `Email` | Preserves the first text element of the user name and the domain. |
| `Phone` | Preserves the area code and last four digits of a Brazilian landline number. |
| `Mobile` | Preserves the area code and last four digits of a Brazilian mobile number. |
| `Cep` | Preserves the first five digits. |
| `Address` | Preserves the first letter of each word and punctuation. |
| `BirthDate` | Hides the day and month while preserving the year. |
| `IpAddress` | IPv4: preserves the first octet. IPv6: preserves the first group. |
| `CreditCard` | Preserves the last four digits for values containing 15 to 19 digits. |
| `BankAccount` | Masks digits while keeping separators. |
| `Pix` | For email keys, preserves the first text element of the user name and domain; for other keys, preserves the first text element. |
| `Salary` | Masks the entire value. |
| `Health` | Masks the entire value. |
| `Biometric` | Masks the entire value. |
| `Password` | Masks the entire value. |
| `Token` | Masks the entire value. |

The rules are designed around the formats described above. For Brazilian identifiers and contact data, provide values in the expected format so the corresponding rule can recognize them.

## Mask strategies

The `MaskStrategy` enum controls how a property is masked.

| Strategy | Behavior |
|---|---|
| `Partial` | Applies the rule defined by the property's `MaskType`. This is the default strategy. |
| `Full` | Masks every text element in the value. |
| `FirstCharacters` | Preserves the first text element and masks the remaining text elements. |
| `LastCharacters` | Preserves the last text element and masks the preceding text elements. |
| `Custom` | Calls a developer-provided static method. |

### Use a specific strategy

```csharp
public class Employee
{
    [Mask(MaskType.Name, MaskStrategy.Full)]
    public string Name { get; set; }

    [Mask(MaskType.Email, MaskStrategy.Partial)]
    public string Email { get; set; }

    [Mask(MaskType.Cpf, MaskStrategy.LastCharacters)]
    public string Document { get; set; }
}
```

When only a mask type is supplied, `Partial` is used:

```csharp
[Mask(MaskType.Cpf)]
public string Cpf { get; set; }
```

## Nested objects and collections

Masking is recursive. When the root object contains another object, an array, or a supported list, the library clones and visits those values too. Attributes on nested object properties are applied in the same way as attributes on root properties.

```csharp
public class Customer
{
    [Mask(MaskType.Name)]
    public string Name { get; set; }

    public Address Address { get; set; }

    public List<Address> Addresses { get; set; }
}

public class Address
{
    [Mask(MaskType.Address)]
    public string Street { get; set; }

    [Mask(MaskType.Cep)]
    public string ZipCode { get; set; }
}
```

Call the masker once on the root object:

```csharp
var maskedCustomer = DataMasker.Mask(customer);
```

The nested `Address` object and each supported item in `Addresses` are cloned and masked. Null collection items remain null.

### Collection and reference behavior

- One-dimensional arrays are supported.
- Collections implementing `IList`, including `List<T>`, are supported.
- The collection itself and its supported elements are cloned.
- Null items are preserved.
- If multiple collection entries point to the same object, the cloned entries keep pointing to the same cloned object.
- Circular references are tracked during cloning to avoid endless recursion.

## Custom masking

Use the custom constructor of `MaskAttribute` to call a static method that receives and returns a `string`.

```csharp
public static class ApplicationMasks
{
    public static string MaskEmployeeCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return value.Length <= 2
            ? new string('*', value.Length)
            : value.Substring(0, 2) + new string('*', value.Length - 2);
    }
}

public class Employee
{
    [Mask(MaskType.Custom, typeof(ApplicationMasks), nameof(ApplicationMasks.MaskEmployeeCode))]
    public string Code { get; set; }
}
```

The configured method must be static and compatible with a `string` input and `string` return value. Configuration or invocation errors are surfaced as exceptions rather than silently ignored.

## Invalid and empty values

The library distinguishes between empty input and malformed structured input:

- `null`, empty strings, and strings containing only whitespace are preserved.
- For structured mask types (such as CPF, CNPJ, RG, email, phone, CEP, date, IP address, and credit card), a non-empty value that does not match the expected format is masked completely.
- This fail-closed behavior helps prevent malformed input from accidentally exposing its original contents.
- `None` and `Custom` with the partial strategy retain their explicitly defined behavior.

Masking is not input validation. Validate data separately before storing or processing it.

## Object creation and custom factory

The standard overload requires a public parameterless constructor on the root type:

```csharp
var masked = DataMasker.Mask(customer);
```

When the root object needs a different creation strategy, provide a factory:

```csharp
var masked = DataMasker.Mask(customer, source => new Customer
{
    // Initialize the new root instance when needed.
});
```

The factory must return a new instance, not the original source object. Returning the source instance causes an `InvalidOperationException`. The factory should not modify the source object.

Nested complex objects also need public parameterless constructors when the library has to create their clones.

## Supported members and limitations

The library uses public properties to traverse and copy objects.

- Only public instance properties with public getters and setters are copied and traversed.
- Indexers, static properties, fields, read-only properties, and write-only properties are ignored.
- Nested complex types must have a public parameterless constructor when a clone must be created.
- Supported collections are one-dimensional arrays and implementations of `IList`.
- Dictionaries, `ISet<T>` (including `HashSet<T>`), custom enumerable-only collections, and read-only collections are not traversed as supported lists.
- Value types and supported immutable/framework types are treated as values rather than recursively traversed objects.
- The optional root factory must return a different instance from the source.

If your model depends on a member or collection type outside these supported cases, review the resulting object carefully and consider converting it to a supported model before masking.

## Security considerations

Canducci.DataMasker is a **data-masking utility**, not an encryption or secure-erasure mechanism.

- The source object remains in memory with its original values.
- The returned object contains masked values only for properties configured with `[Mask]`; unmarked properties are copied.
- Do not treat masked output as irreversible anonymization or as a substitute for access control, encryption, secure storage, or data-retention policies.
- Avoid logging or exposing the original object when the goal is to protect sensitive information.
- Review which properties should be masked in your application, including nested models and newly added fields.

## Running the tests

Clone the repository and run the test suite:

```bash
git clone https://github.com/fulviocanducci/Canducci.DataMasker.git
cd Canducci.DataMasker
dotnet test Canducci.DataMasker.slnx --configuration Release
```

The repository includes automated tests executed through GitHub Actions.

## Contributing

Issues, suggestions, and pull requests are welcome.

- Repository: https://github.com/fulviocanducci/Canducci.DataMasker
- NuGet: https://www.nuget.org/packages/Canducci.DataMasker/

When reporting an issue, include a small reproducible example, the expected behavior, and the actual behavior.

## License

See the license information included in the repository.
