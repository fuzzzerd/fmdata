---
layout: default
nav_order: 1
title: Welcome
---

[![fmdata logo, a C# client for FileMaker](https://raw.githubusercontent.com/fuzzzerd/fmdata/master/images/color-cropped.png)](https://github.com/fuzzzerd/fmdata)

## Packages

| Package | Build Status | MyGet | Nuget |
|---|---|---|---|
| FMData | [![.NET CI Build](https://github.com/fuzzzerd/fmdata/actions/workflows/dotnet.yml/badge.svg)](https://github.com/fuzzzerd/fmdata/actions/workflows/dotnet.yml)| [![Myget](https://img.shields.io/myget/filemaker/vpre/FMData.svg)](https://www.myget.org/feed/filemaker/package/nuget/FMData) | [![NuGet](https://buildstats.info/nuget/fmdata)](https://www.nuget.org/packages/FMData/) |
| FMData.Rest | [![.NET CI Build](https://github.com/fuzzzerd/fmdata/actions/workflows/dotnet.yml/badge.svg)](https://github.com/fuzzzerd/fmdata/actions/workflows/dotnet.yml) | [![MyGet Pre Release](https://img.shields.io/myget/filemaker/vpre/FMData.Rest.svg)](https://www.myget.org/feed/filemaker/package/nuget/FMData.Rest) | [![NuGet](https://buildstats.info/nuget/fmdata.rest)](https://www.nuget.org/packages/FMData.Rest/) |
| FMData.Rest.Auth.FileMakerCloud  | [![.NET CI Build](https://github.com/fuzzzerd/fmdata/actions/workflows/dotnet.yml/badge.svg)](https://github.com/fuzzzerd/fmdata/actions/workflows/dotnet.yml) | [![FMData.Rest.Auth.FileMakerCloud](https://img.shields.io/myget/filemaker/vpre/FMData.Rest.Auth.FileMakerCloud.svg)](https://www.myget.org/feed/filemaker/package/nuget/FMData.Rest.Auth.FileMakerCloud) | [![NuGet](https://buildstats.info/nuget/fmdata.rest.auth.filemakercloud)](https://www.nuget.org/packages/FMData.Rest.Auth.FileMakerCloud/) |
| FMData.Xml  | [![.NET CI Build](https://github.com/fuzzzerd/fmdata/actions/workflows/dotnet.yml/badge.svg)](https://github.com/fuzzzerd/fmdata/actions/workflows/dotnet.yml) | [![FMData.Xml](https://img.shields.io/myget/filemaker/vpre/FMData.Xml.svg)](https://www.myget.org/feed/filemaker/package/nuget/FMData.Xml/) | [![NuGet](https://buildstats.info/nuget/fmdata.xml)](https://www.nuget.org/packages/FMData.Xml/) |

There are plenty of ways to consume RESTful APIs from .NET, but the goal of this project is to provide a blended FileMaker-idiomatic and .NET-idiomatic experience for developers consuming data from FileMaker databases in .NET applications.

The project is organized as three main packages, with a child Auth package for FileMaker Cloud:

- `FMData` is the core and it contains the base and abstract classes utilized by the other implementations.
- `FMData.Rest` is for the Data API and
  - `FMData.Rest.Auth.FileMakerCloud` is used for authentication to the Data API hosted by FileMaker Cloud
- `FMData.Xml` is for consuming the legacy Xml/CWP API.

> *Note: Xml support is experimental, if you need full cwp/xml coverage [check out fmDotNet](https://github.com/fuzzzerd/fmdotnet).*

If you've found a bug, please submit a bug report. If you have a feature idea, open an issue and consider creating a pull request.

## Repository Information

[![FMData repository/commit activity the past year](https://img.shields.io/github/commit-activity/y/fuzzzerd/fmdata.svg)](https://github.com/fuzzzerd/fmdata/commits/master)

[![FMData issues](https://img.shields.io/github/issues/fuzzzerd/fmdata.svg)](https://github.com/fuzzzerd/fmdata/issues)

[![CodeFactor](https://www.codefactor.io/repository/github/fuzzzerd/fmdata/badge)](https://www.codefactor.io/repository/github/fuzzzerd/fmdata)

[![Code size in bytes](https://img.shields.io/github/languages/code-size/fuzzzerd/fmdata.svg)](https://github.com/fuzzzerd/fmdata/commits/master)

[![Language Count](https://img.shields.io/github/languages/count/fuzzzerd/fmdata.svg)](https://github.com/fuzzzerd/fmdata/commits/master)

[![license](https://img.shields.io/github/license/fuzzzerd/fmdata.svg)](https://github.com/fuzzzerd/fmdata/blob/master/LICENSE)

## Versioning

We use [Semantic Versioning](http://semver.org/). Using the Major.Minor.Patch syntax, we attempt to follow the basic rules

 1. MAJOR version when you make incompatible API changes,
 2. MINOR version when you add functionality in a backwards-compatible manner, and
 3. PATCH version when you make backwards-compatible bugfixes.
