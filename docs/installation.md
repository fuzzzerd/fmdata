---
layout: default
nav_order: 2
title: Installation
---

## Installation

Install via `dotnet add` or NuGet. Stable releases are on [NuGet](https://www.nuget.org/packages?q=FMData) and CI builds are on [MyGet](https://www.myget.org/feed/filemaker/package/nuget/FMData).

### Packages

**FMData** — Core abstractions and base classes used by all implementations.

```sh
dotnet add package FMData
```

**FMData.Rest** — FileMaker Data API client. This is the package most projects need.

```sh
dotnet add package FMData.Rest
```

**FMData.Rest.Auth.FileMakerCloud** — Authentication provider for FileMaker Cloud (Claris Connect) via AWS Cognito.

```sh
dotnet add package FMData.Rest.Auth.FileMakerCloud
```

**FMData.Xml** — Client for the legacy XML/CWP API (experimental).

```sh
dotnet add package FMData.Xml
```

> *Note: If you need full CWP/XML coverage, [check out fmDotNet](https://github.com/fuzzzerd/fmdotnet).*

### Supported Frameworks

| Package | Target Frameworks |
|---|---|
| FMData | `net45`, `netstandard1.3`, `netstandard2.0`, `net6.0`, `net8.0` |
| FMData.Rest | `net45`, `netstandard1.3`, `netstandard2.0`, `net6.0`, `net8.0` |
| FMData.Rest.Auth.FileMakerCloud | `netstandard2.0`, `net6.0`, `net8.0` |
| FMData.Xml | `netstandard2.0`, `net6.0`, `net8.0` |

### Pre-release Builds

Pre-release packages are published to MyGet. Add the feed to your NuGet sources:

```sh
dotnet nuget add source https://www.myget.org/F/filemaker/api/v3/index.json -n filemaker-myget
```

Then install with the `--prerelease` flag:

```sh
dotnet add package FMData.Rest --prerelease
```
