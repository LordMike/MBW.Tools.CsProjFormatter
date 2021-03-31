# MBW.Tools.CsProjFormatter [![Generic Build](https://github.com/LordMike/MBW.Tools.CsProjFormatter/actions/workflows/dotnet.yml/badge.svg)](https://github.com/LordMike/MBW.Tools.CsProjFormatter/actions/workflows/dotnet.yml) [![NuGet](https://img.shields.io/nuget/v/MBW.Tools.CsProjFormatter.svg)](https://www.nuget.org/packages/MBW.Tools.CsProjFormatter) [![GHPackages](https://img.shields.io/badge/package-alpha-green)](https://github.com/LordMike/MBW.Tools.CsProjFormatter/packages/703133)

Dotnet tool to format `.csproj` files. 

## Installation

Run `dotnet tool install -g MBW.Tools.CsProjFormatter`. After this, `csproj-format` should be in your PATH.

## Usage

Run `csproj-format <dir>` to recursively format all `.csproj` and `.targets` files in that directory. Multiple directories can be specified. The help page (`--help`) details more options that can be set, f.ex. dry-run (`-n`) or including/excluding more file types (`--include` and `--exclude`).

## Configuration

The formatter reads all appropriate `.editorconfig` files as specified by that standard (reading all parent editor configs etc.). Configure the options for `.csproj` and `.targets` as appropriate.

### Options

#### indent_style and indent_size

The formatter respects the `indent_style` and `indent_size` options. If specified, indentation of all formatted files will be as specified. 

These are part of the ["default" options](https://editorconfig-specification.readthedocs.io/en/latest/#supported-properties).

#### end_of_line

The formatter respects the `end_of_line` option. If specified, all line endings will be converted to match the specified value. Supported values: `cr`, `lf`, `crlf`

This is part of the ["default" options](https://editorconfig-specification.readthedocs.io/en/latest/#supported-properties).

#### charset

The formatter respects the `charset` option. If specified, all files will be rewritten in the specified encoding. Supported values: `latin1`, `utf-16be`, `utf-16le`, `utf-8`, `utf-8-bom`.

This is part of the ["default" options](https://editorconfig-specification.readthedocs.io/en/latest/#supported-properties).

#### insert_final_newline

The formatter respects the `insert_final_newline` option. If specified, all files will include a trailing empty line.

This is part of the ["default" options](https://editorconfig-specification.readthedocs.io/en/latest/#supported-properties).


#### csproj_prefer_package_reference_attributes

If `csproj_prefer_package_reference_attributes` is specified to `true`, the formatter will convert all "verbose" package references to more compact ones. 

Example:
```xml
<PackageReference>
  <Include>My.Package</Include>
  <Version>3.1.1</Version>
<PackageReference>
```

Becomes:

```xml
<PackageReference Include="My.Package" Version="3.1.1" />
```

#### csproj_sort_package_project_references

If `csproj_sort_package_project_references` is specified to `true`, the formatter will sort all ItemGroups that include ONLY `PackageReference` and `ProjectReference` statements. 

Example:
```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="2.2.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="3.1.1" />
    <PackageReference Include="McMaster.Extensions.CommandLineUtils" Version="2.3.4" />
    <PackageReference Include="Serilog.Extensions.Logging" Version="2.0.4" />
    <PackageReference Include="Microsoft.Extensions.FileSystemGlobbing" Version="2.2.0" />
  </ItemGroup>
```

Becomes:

```xml
  <ItemGroup>
    <PackageReference Include="McMaster.Extensions.CommandLineUtils" Version="2.3.4" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="2.2.0" />
    <PackageReference Include="Microsoft.Extensions.FileSystemGlobbing" Version="2.2.0" />
    <PackageReference Include="Serilog.Extensions.Logging" Version="2.0.4" />
    <PackageReference Include="Serilog.Sinks.Console" Version="3.1.1" />
  </ItemGroup>
```

#### csproj_split_top_level_elements

If `csproj_split_top_level_elements` is specified to `true`, the formatter will split all top-level declarations by one empty line.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netcoreapp2.2</TargetFramework>
    <Version>0.1.0</Version>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="My.Package" Version="1.2.3.4" />
  </ItemGroup>
</Project>
```

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netcoreapp2.2</TargetFramework>
    <Version>0.1.0</Version>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="My.Package" Version="1.2.3.4" />
  </ItemGroup>

</Project>
```
