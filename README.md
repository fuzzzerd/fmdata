# FMData A C# Client for FileMaker

FMData is a C# client wrapper library for accessing data from FileMaker databases. It currently supports the FileMaker Data API. Using the client is simple and straight forward and exposes a blended experience between C# norms and FileMaker standards.

## About FMData

There are plenty of ways to consume RESTful apis from .NET; but the goal of this project is to provide a blended FileMaker-idiomatic and .NET-idiomatic interface for consuming data in FileMaker databases.

To that end, the project is organized in three packages. `FMData` is the core. It contains the abstract classes utilized by the other implementations. `FMdata.Rest` is for the Data API and `FMData.Xml` is for consuming the legacy Xml/CWP API.

If you've found a bug, please submit a bug report. If you have a feature idea, open an issue and consider creating a pull request.

> Note: Xml support is experimental, if you need full cwp/xml coverage check out fmDotNet. 

> Note: I'm curious if a wrapper accessing the ODBC interface to FileMaker would be worth while?

| Package | Build Status |  MyGet | Nuget |
|---|---|---|---|---|---|
| FMData | [![Build status](https://ci.appveyor.com/api/projects/status/nnqby0f5rpcsl3uv?svg=true)](https://ci.appveyor.com/project/fuzzzerd/fmdata) | [![MyGet](https://img.shields.io/myget/filemaker/dt/fmdata.svg?style=flat-square)](https://www.myget.org/feed/filemaker/package/nuget/FMData) | [![NuGet](https://img.shields.io/nuget/dt/FMData.svg?style=flat-square)](https://www.nuget.org/packages/FMData/) |
| FMData.Rest | [![Build status](https://ci.appveyor.com/api/projects/status/nnqby0f5rpcsl3uv?svg=true)](https://ci.appveyor.com/project/fuzzzerd/fmdata) | [![MyGet](https://img.shields.io/myget/filemaker/dt/fmdata.rest.svg?style=flat-square)](https://www.myget.org/feed/filemaker/package/nuget/FMData.Rest) | [![NuGet](https://img.shields.io/nuget/dt/FMData.Rest.svg?style=flat-square)](https://www.nuget.org/packages/FMData.Rest/)|
| FMData.Xml | [![Build status](https://ci.appveyor.com/api/projects/status/nnqby0f5rpcsl3uv?svg=true)](https://ci.appveyor.com/project/fuzzzerd/fmdata) | [![MyGet](https://img.shields.io/myget/filemaker/dt/fmdata.xml.svg?style=flat-square)](https://www.myget.org/feed/filemaker/package/nuget/FMData.Xml) | [![NuGet](https://img.shields.io/nuget/dt/FMData.Xml.svg?style=flat-square)](https://www.nuget.org/packages/FMData.Xml/)|

| Activity | License |
| ---- | ---- |
| [![FMData repository/commit activity](https://img.shields.io/github/commit-activity/w/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/commits/master)|[![license](https://img.shields.io/github/license/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/blob/master/LICENSE) |

-----

## Installation

Install via `dotnet add` or nuget. Stable releases are on NuGet and CI builds are on MyGet.

    dotnet add package FMData.Rest

## Example Usage

The recommended way to consume this library is using a strongly typed model as follows.

### Setting up your model

A model should roughly match a table in your solution. Its accessed via layout.

```csharp
[DataContract(Name="NameOfYourLayout")] // use the DataContract attribute to specify the layout
public class Model
{
    public string Name { get; set; }
    // if your model name does not match
    // use DataMember
    [DataMember(Name="overrideFieldName")] // the filemaker field to use
    public string Address { get; set; }

    // if your model has properties you don't want mapped use
    [NotMapped] // to skip mapping of the field
    public string NotNeededField { get; set; }
}
```

### Performing a Find

```csharp
var client = new FileMakerRestClient("server", "fileName", "user", "pass"); // without .fmp12
var toFind = new Model { Name = "someName" };
var results = await client.FindAsync(toFind);
// results = IEnumerable<Model> matching with Name field matching "someName" as a FileMaker Findrequest.
```

### Create a new record

```csharp
var client = new FileMakerRestClient("server", "fileName", "user", "pass"); // without .fmp12
var toCreate = new Model { Name = "someName", Address = "123 Main Street" };
var results  = await client.CreateAsync(toCreate);
//  results is an ICreateResponse which indicates success (0/OK or Failure with FMS code/message)
```

### Updating a record

```csharp
var client = new FileMakerRestClient("server", "fileName", "user", "pass"); // without .fmp12
var fileMakerRecordId = 1; // this is the value from the calculation: Get(RecordID)
var toUpdate = new Model { Name = "someName", Address = "123 Main Street" };
var results = await client.EditAsync(fileMakerRecordId, toCreate);
//  results is an IEditResponse which indicates success (0/OK or Failure with FMS code/message)
```

### Find with FileMaker Id Mapping

Note you need to add an int property to the Model `public int FileMakerRecordId { get; set; }` and provide the Func to the `FindAsync` method to tell FMData how to map the FileMaker Id returned from the API to your model.

```csharp
Func<Model, int, object> FMRecordIdMapper = (o, id) => o.FileMakerRecordId = id;
var client = new FileMakerRestClient("server", "fileName", "user", "pass"); // without .fmp12
var toFind = new Model { Name = "someName" };
var results = await client.FindAsync(toFind, FMRecordIdMapper);
// results is IEnumerable<Model> matching with Name field matching "someName" as a FileMaker Findrequest.
```

Alternatively, if you create a calculated field `Get(RecordID)` and put it on your layout, you can map it the normal way.

### Find and load Container Data

Make sure you use the `[ContainerDataFor("NameOfContainer")]` attribute along with a `byte[]` property for processing of your model.

```csharp
var client = new FileMakerRestClient("server", "fileName", "user", "pass"); // without .fmp12
var toFind = new Model { Name = "someName" };
var results = await client.FindAsync(toFind);
await client.ProcessContainers(results); 
// results = IEnumerable<Model> matching with Name field matching "someName" as a FileMaker Findrequest.
```

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

-----

## Repository Statistics

[![FMData repository/commit activity the past year](https://img.shields.io/github/commit-activity/y/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/commits/master)

[![FMData issues](https://img.shields.io/github/issues/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/issues)

[![Code size in bytes](https://img.shields.io/github/languages/code-size/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/commits/master)

[![Language Count](https://img.shields.io/github/languages/count/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/commits/master)

[![license](https://img.shields.io/github/license/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/blob/master/LICENSE)