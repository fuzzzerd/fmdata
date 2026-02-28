---
layout: default
parent: Guide
nav_order: 7
title: Container Data
---

## Container Data

Container fields in FileMaker store files (images, PDFs, etc.). FMData handles downloading and uploading container data separately from regular field data.

### Model Setup

Add a `byte[]` property with `[ContainerDataFor]` referencing the container field's C# property name:

```csharp
[DataContract(Name = "Documents")]
public class Document
{
    [DataMember]
    public string Title { get; set; }

    [DataMember]
    public string Attachment { get; set; }  // container field

    [ContainerDataFor("Attachment")]  // references the C# property name
    public byte[] AttachmentData { get; set; }
}
```

### Downloading Container Data

After finding records, use `ProcessContainers` to download all container data for a collection:

```csharp
var results = await client.FindAsync(new Document { Title = "Report" });
await client.ProcessContainers(results);
// each result's AttachmentData now contains the file bytes
```

For a single record:

```csharp
await client.ProcessContainer(singleDocument);
```

#### Auto-load on Find

Set `LoadContainerData` on a find request to automatically download container data with the results:

```csharp
var req = client.GenerateFindRequest<Document>();
req.Layout = "Documents";
req.AddQuery(new Document { Title = "Report" }, omit: false);
req.LoadContainerData = true;

var results = await client.SendAsync(req);
// container data is already loaded
```

### Uploading Container Data

Use `UpdateContainerAsync` to upload data to a container field:

```csharp
var fileBytes = File.ReadAllBytes("report.pdf");

await client.UpdateContainerAsync(
    "Documents",         // layout
    recordId,            // FileMaker record ID
    "Attachment",        // container field name
    "report.pdf",        // file name
    fileBytes);          // file content
```

For repeating container fields, specify the repetition number:

```csharp
await client.UpdateContainerAsync(
    "Documents", recordId, "Attachment", "report.pdf",
    repetition: 2, content: fileBytes);
```

> **Note:** Creating a record with container data requires two calls — one to create the record, then a second to upload the container data.
