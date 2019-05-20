[![logo](https://repository-images.githubusercontent.com/126491080/72f6dd80-78ac-11e9-9d45-939bae6a8cb5)](https://github.com/fuzzzerd/fmdata)

| Package | Build Status |  MyGet Version | Nuget Version | Nuget Downloads |
|---|---|---|---|---|
| FMData | [![Build status](https://ci.appveyor.com/api/projects/status/nnqby0f5rpcsl3uv?svg=true)](https://ci.appveyor.com/project/fuzzzerd/fmdata) | [![FMData](https://img.shields.io/myget/filemaker/vpre/FMData.svg?style=flat-square)](https://www.myget.org/feed/filemaker/package/nuget/FMData) | [![NuGet](https://img.shields.io/nuget/v/FMData.svg?style=flat-square)](https://www.nuget.org/packages/FMData/) | [![NuGet](https://img.shields.io/nuget/dt/FMData.svg?style=flat-square)](https://www.nuget.org/packages/FMData/)|
| FMData.Rest | [![Build status](https://ci.appveyor.com/api/projects/status/nnqby0f5rpcsl3uv?svg=true)](https://ci.appveyor.com/project/fuzzzerd/fmdata) | [![MyGet Pre Release](https://img.shields.io/myget/filemaker/vpre/FMData.Rest.svg?style=flat-square)](https://www.myget.org/feed/filemaker/package/nuget/FMData.Rest) | [![NuGet](https://img.shields.io/nuget/v/FMData.Rest.svg?style=flat-square)](https://www.nuget.org/packages/FMData.Rest/) | [![NuGet](https://img.shields.io/nuget/dt/FMData.Rest.svg?style=flat-square)](https://www.nuget.org/packages/FMData.Rest/)|
| FMData.Xml  | [![Build status](https://ci.appveyor.com/api/projects/status/nnqby0f5rpcsl3uv?svg=true)](https://ci.appveyor.com/project/fuzzzerd/fmdata) | [![FMData.Xml](https://img.shields.io/myget/filemaker/vpre/FMData.Xml.svg?style=flat-square)](https://www.myget.org/feed/filemaker/package/nuget/FMData.Xml/) | [![NuGet](https://img.shields.io/nuget/v/FMData.Xml.svg?style=flat-square)](https://www.nuget.org/packages/FMData.Xml/) | [![NuGet](https://img.shields.io/nuget/dt/FMData.Xml.svg?style=flat-square)](https://www.nuget.org/packages/FMData.Xml/)|

There are plenty of ways to consume RESTful APIs from .NET, but the goal of this project is to provide a blended FileMaker-idiomatic and .NET-idiomatic experience for developers consuming data from FileMaker databases in .NET applications.

The project is organized as three packages.

- `FMData` is the core and it contains the base and abstract classes utilized by the other implementations.
- `FMdata.Rest` is for the Data API and
- `FMData.Xml` is for consuming the legacy Xml/CWP API.

> *Note: Xml support is experimental, if you need full cwp/xml coverage [check out fmDotNet](https://github.com/fuzzzerd/fmdotnet).*

If you've found a bug, please submit a bug report. If you have a feature idea, open an issue and consider creating a pull request.

| Chat | Tests | Grade | Activity | License |
| ---- | ---- | ---- | ---- | ---- |
| [![Join the chat at https://gitter.im/fmdataio/community](https://badges.gitter.im/fmdataio/community.svg)](https://gitter.im/fmdataio/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)| [![AppVeyor tests branch](https://img.shields.io/appveyor/tests/fuzzzerd/fmdata/dev.svg?style=flat-square)](https://ci.appveyor.com/project/fuzzzerd/fmdata/build/tests) | [![CodeFactor](https://www.codefactor.io/repository/github/fuzzzerd/fmdata/badge)](https://www.codefactor.io/repository/github/fuzzzerd/fmdata) | [![FMData repository/commit activity](https://img.shields.io/github/commit-activity/w/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/commits/master)|[![license](https://img.shields.io/github/license/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/blob/master/LICENSE) |

-----
> *Version 4 has several breaking changes. Please [review the changes](https://github.com/fuzzzerd/fmdata/milestone/5?closed=1) prior to updating your project.*
-----

## Installation

Install via `dotnet add` or nuget. Stable releases are on NuGet and CI builds are on MyGet.

```ps
dotnet add package FMData.Rest
```

-----

## Example Usage

The recommended way to consume this library is using a strongly typed model as follows.

### Setting up your model

A model should roughly match a table in your solution. Its accessed via layout.

```csharp
// use the DataContract attribute to link your model to a layout
[DataContract(Name="NameOfYourLayout")]
public class Model
{
    [DataMember]
    public string Name { get; set; }

    // if your model name does not match use DataMember
    [DataMember(Name="overrideFieldName")] // the filemaker field to use
    public string Address { get; set; }

    [DataMember]
    public string SomeContainerField { get; set; }

    // use the ContainerDataFor attribute to map container data to a byte[]
    [ContainerDataFor("SomeContainerField")] // use the name in your C# model
    public byte[] DataForSomeContainerField { get; set; }

    // if your model has properties you don't want mapped use
    [IgnoreDataMember] // to skip mapping of the field
    public string NotNeededField { get; set; }
}
```

### Using IHttpClientFactory

Constructors take an `HttpClient` and you can setup the DI pipeline in Startup.cs like so.

```csharp
services.AddSingleton<FMData.ConnectionInfo>(ci => new FMData.ConnectionInfo
{
    FmsUri = "https://example.com",
    Username = "user",
    Password = "password",
    Database = "FILEMAKERFILE"
});
services.AddHttpClient<IFileMakerApiClient, FileMakerRestClient>();
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

Alternatively, if you create a calculated field `Get(RecordID)` and put it on your layout then map it the normal way.

### Find and load Container Data

Make sure you use the `[ContainerDataFor("NameOfContainer")]` attribute along with a `byte[]` property for processing of your model.

```csharp
var client = new FileMakerRestClient("server", "fileName", "user", "pass"); // without .fmp12
var toFind = new Model { Name = "someName" };
var results = await client.FindAsync(toFind);
await client.ProcessContainers(results);
// results = IEnumerable<Model> matching with Name field matching "someName" as a FileMaker Findrequest.
```

### Insert or Update Container Data

```csharp
// assume recordId = a FileMaker RecordId mapped using FMIdMapper
// assume containerDataByteArray is a byte array with file contents of some sort
var client = new FileMakerRestClient("server", "fileName", "user", "pass"); // without .fmp12
_client.UpdateContainerAsync(
    "layout",
    recordId,
    "containerFieldName",
    "filename.jpg/png/pdf/etc",
    containerDataByteArray);
```

> *Note: In order to create a record with container data two calls must be made. One that creates the actual record ( see above) and one that updates the container field contents.*

-----

## FileMaker REST / Data API Documentation

- [FileMaker Data API Documentation (FMS17)](http://fmhelp.filemaker.com/docs/17/en/dataapi/)
- [FileMaker REST API Documentation (FMS16)](https://fmhelp.filemaker.com/docs/16/en/restapi/) -- Not Supported by this project.

## FileMaker CWP with Xml Guide

- [FileMaker Server 16 Web Publishing Guide](https://fmhelp.filemaker.com/docs/16/en/fms16_cwp_guide.pdf)
- [FileMaker Server 15 Web Publishing Guide](https://fmhelp.filemaker.com/docs/15/en/fms15_cwp_guide.pdf)

-----

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

-----
