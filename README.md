# FM Data

FMData is a C# client library for accessing data from FileMaker databases. Currently supporting the FileMaker 17 REST/DATA API.

The interfaces and core elements are defined in the `FMData` package, the implementations for REST API or XML API are contained within their respective packages: `FMData.Rest` and `FMData.Xml` -- the current focus is on completing the Rest implementation. The Xml implementation currently lacks many features.

| Build Status | Activity | MyGet | Nuget | License |
|---|---|---|---|---|
| [![Build status](https://ci.appveyor.com/api/projects/status/nnqby0f5rpcsl3uv?svg=true)](https://ci.appveyor.com/project/fuzzzerd/fmdata) | [![FMData repository/commit activity](https://img.shields.io/github/commit-activity/w/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/commits/master) | [![MyGet](https://img.shields.io/myget/filemaker/dt/fmdata.svg?style=flat-square)](https://www.myget.org/feed/filemaker/package/nuget/FMData) | [![NuGet](https://img.shields.io/nuget/dt/FMData.svg?style=flat-square)](https://www.nuget.org/packages/FMData/) | [![license](https://img.shields.io/github/license/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/blob/master/LICENSE) |

## Installation

Stable builds are released to NuGet and CI builds are released on MyGet.

## Stable Builds are on NuGet

    dotnet add package FMData.Rest
    //or
    dotnet add package FMData.Xml

## Example Usage

    var client = new FileMakerRestClient("server", "fileName", "user", "pass"); // without .fmp12
    var toFind = new Model { Name = "someName" };
    var resuts = await client.FindAsync(toFind);
    // results is IEnumerable<Model> matching with Name field matching "someName" as a FileMaker Findrequest.

## Contributing

- If you've found a bug or have a suggestion, add it to our [![FMData issues](https://img.shields.io/github/issues/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/issues) log.
- Fork the repository and submit a pull request with a bug fix or new feature.

## Planned Features

Check out the issues log to see what features are planned. Feel free to contribute by adding issues or submitting a pull request.

## Upstream Documentation

### FileMaker REST / Data API Documentation

- [FileMaker Data API Documentation (FMS17)](http://fmhelp.filemaker.com/docs/17/en/dataapi/)
- [FileMaker REST API Documentation (FMS16)](https://fmhelp.filemaker.com/docs/16/en/restapi/) -- Not Supported by this project.

### FileMaker CWP with Xml Guide

- [FileMaker Server 16 Web Publishing Guide](https://fmhelp.filemaker.com/docs/16/en/fms16_cwp_guide.pdf)
- [FileMaker Server 15 Web Publishing Guide](https://fmhelp.filemaker.com/docs/15/en/fms15_cwp_guide.pdf)

## Versioning

We use [Semantic Versioning](http://semver.org/). Using the Major.Minor.Patch syntax, we attempt to follow the basic rules

 1. MAJOR version when you make incompatible API changes,
 2. MINOR version when you add functionality in a backwards-compatible manner, and
 3. PATCH version when you make backwards-compatible bug fixes.

## Repository Statistics

[![FMData repository/commit activity the past year](https://img.shields.io/github/commit-activity/y/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/commits/master)

[![FMData issues](https://img.shields.io/github/issues/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/issues)

[![Code size in bytes](https://img.shields.io/github/languages/code-size/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/commits/master)

[![Language Count](https://img.shields.io/github/languages/count/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/commits/master)

[![license](https://img.shields.io/github/license/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/blob/master/LICENSE)
