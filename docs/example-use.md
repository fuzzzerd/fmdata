---
layout: default
nav_order: 3
title: Example Use
---

## Example Usage

The recommended way to consume this library is using a strongly typed model as follows.

Please review the /tests/FMData.Rest.Tests/ project folder for expected usage flows.

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
    [DataMember(Name="overrideFieldName")] // the internal database field to use
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

Constructors take an `HttpClient` and you can setup the DI pipeline in Startup.cs like so for standard use:

```csharp
services.AddSingleton<FMData.ConnectionInfo>(ci => new FMData.ConnectionInfo
{
    FmsUri = "https://example.com",
    Username = "user",
    Password = "password",
    Database = "FILE_NAME"
});
services.AddHttpClient<IFileMakerApiClient, FileMakerRestClient>();
```

If you prefer to use a singleton instance of `IFileMakerApiClient` you have to do a little bit more work in startup. This can improve performance if you're making lots of hits to the Data API over a single request to your application:

```csharp
services.AddHttpClient(); // setup IHttpClientFactory in the DI container
services.AddSingleton<FMData.ConnectionInfo>(ci => new FMData.ConnectionInfo
{
    FmsUri = "https://example.com",
    Username = "user",
    Password = "password",
    Database = "FILE_NAME"
});
// Keep the FileMaker client as a singleton for speed
services.AddSingleton<IFileMakerApiClient, FileMakerRestClient>(s => {
    var hcf = s.GetRequiredService<IHttpClientFactory>();
    var ci = s.GetRequiredService<ConnectionInfo>();
    return new FileMakerRestClient(hcf.CreateClient(), ci);
});
```

Behind the scenes, the injected `HttpClient` is kept alive for the lifetime of the FMData client (rest/XML) and reused throughout. This is useful to manage the lifetime of `IFileMakerApiClient` as a singleton, since it stores data about FileMaker Data API tokens and reuses them as much as possible. Simply using `services.AddHttpClient<IFileMakerApiClient, FileMakerRestClient>();` keeps the lifetime of our similar to that of a 'managed `HttpClient`' which works for simple scenarios.

Test both approaches in your solution and use what works.

### Authentication with FileMaker Cloud

We can use the `FileMakerRestClient`, when the setup is done. Just create a new `ConnectionInfo` object and set the required properties:

```cs
var conn = new ConnectionInfo();
conn.FmsUri = "https://{NAME}.account.filemaker-cloud.com";
conn.Username = "user@domain.com";
conn.Password = "********";
conn.Database = "Reporting";
```

Then instantiate the `FileMakerRestClient` with a `FileMakerCloudAuthTokenProvider` as follows:

```cs
var fm = new FileMakerRestClient(new HttpClient(), new FileMakerCloudAuthTokenProvider(conn));
```

For a full description of using FileMaker Data API with FileMaker Cloud, [see this comment](https://github.com/fuzzzerd/fmdata/issues/217#issuecomment-1203202293).

### Performing a Find

```csharp
var client = new FileMakerRestClient("server", "fileName", "user", "pass"); // without .fmp12
var toFind = new Model { Name = "someName" };
var results = await client.FindAsync(toFind);
// results = IEnumerable<Model> matching with Name field matching "someName" as a FileMaker FindRequest.
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

### Find with FileMaker ID Mapping

Note you need to add an int property to the Model `public int FileMakerRecordId { get; set; }` and provide the Func to the `FindAsync` method to tell FMData how to map the FileMaker ID returned from the API to your model.

```csharp
Func<Model, int, object> FMRecordIdMapper = (o, id) => o.FileMakerRecordId = id;
var client = new FileMakerRestClient("server", "fileName", "user", "pass"); // without .fmp12
var toFind = new Model { Name = "someName" };
var results = await client.FindAsync(toFind, FMRecordIdMapper);
// results is IEnumerable<Model> matching with Name field matching "someName" as a FileMaker FindRequest.
```

### Find with Data Info

```csharp
var toFind = new Model { Name = "someName" };
var req = new FindRequest<Model>() { Layout = layout };
req.AddQuery(toFind, false);
var (data, info) = await fdc.SendAsync(req, true);
```

Alternatively, if you create a calculated field `Get(RecordID)` and put it on your layout then map it the normal way.

### Find with Portal Limit and Offset

By default, the FileMaker Data API limits portal records to 50 per request. You can control per-portal `limit` and `offset` using the fluent `WithPortal` builder or `ConfigurePortal` method on `FindRequest<T>`.

Use the fluent builder to chain portal configuration:

```csharp
var req = new FindRequest<Model>() { Layout = "layout" };
req.AddQuery(new Model { Name = "someName" }, false);

// configure portals with limit and offset
req.WithPortal("RelatedInvoices").Limit(100).Offset(1)
   .WithPortal("LineItems").Limit(200);

var results = await client.SendAsync(req);
```

Or use `ConfigurePortal` directly:

```csharp
var req = new FindRequest<Model>() { Layout = "layout" };
req.AddQuery(new Model { Name = "someName" }, false);
req.ConfigurePortal("RelatedInvoices", limit: 100, offset: 1);
var results = await client.SendAsync(req);
```

To include specific portals in the response without setting limits:

```csharp
var req = new FindRequest<Model>() { Layout = "layout" };
req.AddQuery(new Model { Name = "someName" }, false);
req.IncludePortals("RelatedInvoices", "LineItems");
var results = await client.SendAsync(req);
```

Portal parameters work with both find requests (POST to `_find`) and empty-query get-records requests (GET).

### Find and load Container Data

Make sure you use the `[ContainerDataFor("NameOfContainer")]` attribute along with a `byte[]` property for processing of your model.

```csharp
var client = new FileMakerRestClient("server", "fileName", "user", "pass"); // without .fmp12
var toFind = new Model { Name = "someName" };
var results = await client.FindAsync(toFind);
await client.ProcessContainers(results);
// results = IEnumerable<Model> matching with Name field matching "someName" as a FileMaker FindRequest.
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
