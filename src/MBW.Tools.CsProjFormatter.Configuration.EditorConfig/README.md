## CSProj Formatter: Editorconfig support

Example editorconfig.

```editorconfig
[*.csproj]
indent_style = space
indent_size = 2

[*.targets]
end_of_line = cr
```

## Supported configuration values

### EditorConfig defaults

The following ["default" set of Editor Config values](https://github.com/editorconfig/editorconfig/wiki/EditorConfig-Properties#current-universal-properties) are supported. 

#### indent_style

* Values: `space`, `tab`
* Default: `space`

Controls indentation of the XML.

#### indent_size

* Values: `numerical`
* Default: `2`

Number of `indent_style` to place for each level of indentation.

#### end_of_line

* Values: `cr`, `crlf`, `lf`
* Default: system default
* Only applicable when working with files

Controls which line ending to place.

#### charset

* Values: `latin1` (aka. iso-8859-1), `utf-8`, `utf-8-bom`, `utf-16be`, `utf-16le`
* Default: `utf-8`
* Only applicable when working with files

Determines which encoding to write the file in.

#### insert_final_newline

* Values: `true`, `false`
* Default: `true` 
* Only applicable when working with files

If set, formatted files will end with a newline.

### Csproj Specific

#### csproj_prefer_package_reference_attributes

* Values: `true`, `false`
* Default: `true`

When `PackageReference` or `ProjectReference` elements are encounted, where values are set using elements rather than attributes, the values are converted to use attributes.

```xml
  <!-- Original -->
  <ItemGroup>
    <PackageReference>
      <Include>MyPackage</Include>
      <Version>1.2.34</Version>
    </PackageReference>
  </ItemGroup>
  
  <!-- Becomes -->
  <ItemGroup>
    <PackageReference Include="MyPackage" Version="1.2.34" />
  </ItemGroup>
```

#### csproj_sort_package_project_references

* Values: `true`, `false`
* Default: `true`

Sorts `ItemGroup`'s which contain _only_ `PackageReference` and `ProjectReference` elements.

```xml
  <!-- Original -->
  <ItemGroup>
    <!-- Some comment -->
    <PackageReference Include="Beta" Version="1.2.34" />
    <PackageReference Include="Alpha" Version="1.2.34" />
  </ItemGroup>
  
  <!-- Becomes -->
  <ItemGroup>
    <PackageReference Include="Alpha" Version="1.2.34" />
    <!-- Some comment -->
    <PackageReference Include="Beta" Version="1.2.34" />
  </ItemGroup>
```

#### csproj_split_top_level_elements

* Values: `true`, `false`
* Default: `true`

Splits all top-level elements in a `<Project>`, so that there is one empty line between them.

```xml
  <!-- Original -->
  <Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
      <TargetFramework>netcoreapp2.2</TargetFramework>
    </PropertyGroup>
    <PropertyGroup>
      <Version>1.2.3</Version>
    </PropertyGroup>
  </Project>
  
  <!-- Becomes -->
  <Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
      <TargetFramework>netcoreapp2.2</TargetFramework>
    </PropertyGroup>

    <PropertyGroup>
      <Version>1.2.3</Version>
    </PropertyGroup>

  </Project>
```