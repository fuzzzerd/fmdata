---
layout: default
parent: Guide
nav_order: 2
title: Finding Records
---

## Finding Records

### Simple Find

Set properties on a model instance to define search criteria:

```csharp
var query = new Invoice { Status = "Open" };
var results = await client.FindAsync(query);
```

### Pagination

Use `skip` and `take` parameters to paginate results:

```csharp
var results = await client.FindAsync(query, skip: 0, take: 50);
```

### Record ID Mapping

Pass `Func` delegates to map FileMaker's internal record ID and modification ID onto your model:

```csharp
Func<Invoice, int, object> fmIdFunc = (o, id) => o.FileMakerRecordId = id;
Func<Invoice, int, object> fmModIdFunc = (o, id) => o.FileMakerModId = id;

var results = await client.FindAsync(query, skip: 0, take: 50,
    script: null, scriptParameter: null,
    fmIdFunc: fmIdFunc, fmModIdFunc: fmModIdFunc);
```

### Layout Override

By default, FMData uses the layout from `[DataContract(Name = "...")]`. To query a different layout:

```csharp
var results = await client.FindAsync("AlternateLayout", query);
```

### Find with Scripts

Run a FileMaker script as part of a find request:

```csharp
var results = await client.FindAsync(query, skip: 0, take: 100,
    script: "AuditLog", scriptParameter: "find",
    fmIdFunc: fmIdFunc);
```

### Get a Single Record by ID

Retrieve a single record using its FileMaker record ID:

```csharp
var record = await client.GetByFileMakerIdAsync<Invoice>(42, fmIdFunc, fmModIdFunc);
```

You can also specify a layout override:

```csharp
var record = await client.GetByFileMakerIdAsync<Invoice>("Invoices", 42, fmIdFunc);
```

### Advanced: SendAsync with IFindRequest

For full control, build an `IFindRequest` directly:

```csharp
var req = client.GenerateFindRequest<Invoice>();
req.Layout = "Invoices";
req.AddQuery(new Invoice { Status = "Open" }, omit: false);

var results = await client.SendAsync(req);
```

#### Omit Queries

Add queries with `omit: true` to exclude matching records:

```csharp
req.AddQuery(new Invoice { Status = "Open" }, omit: false);
req.AddQuery(new Invoice { Status = "Cancelled" }, omit: true);
```

#### Sorting

Add sort fields with direction (`"ascend"` or `"descend"`):

```csharp
req.AddSort("InvoiceDate", "descend");
req.AddSort("InvoiceNumber", "ascend");
```

### Advanced: SendAsync with DataInfo and Script Results

Use the `includeDataInfo` overload to get record count metadata and script results alongside your data:

```csharp
var req = client.GenerateFindRequest<Invoice>();
req.Layout = "Invoices";
req.AddQuery(new Invoice { Status = "Open" }, omit: false);

var (data, dataInfo, scriptResponse) = await client.SendAsync(req, includeDataInfo: true);

// dataInfo.FoundCount, dataInfo.TotalRecordCount, dataInfo.ReturnedCount
// scriptResponse?.ScriptResult, scriptResponse?.ScriptError
```

### Advanced: SendFindRequestAsync

Use `SendFindRequestAsync` when the request model differs from the response model:

```csharp
var req = client.GenerateFindRequest<InvoiceQuery>();
req.Layout = "Invoices";
req.AddQuery(new InvoiceQuery { Status = "Open" }, omit: false);

Func<InvoiceDetail, int, object> fmIdFunc = (o, id) => o.RecordId = id;
Func<InvoiceDetail, int, object> fmModIdFunc = (o, id) => o.ModId = id;

var (data, dataInfo, scriptResponse) = await client.SendFindRequestAsync<InvoiceDetail, InvoiceQuery>(
    req, fmIdFunc, fmModIdFunc);
```
