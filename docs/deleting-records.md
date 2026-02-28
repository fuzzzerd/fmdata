---
layout: default
parent: Guide
nav_order: 5
title: Deleting Records
---

## Deleting Records

### Delete by Model Type

Delete using the layout from the model's `[DataContract]` attribute:

```csharp
var response = await client.DeleteAsync<Invoice>(recordId);
```

### Delete by Layout and Record ID

Specify the layout explicitly:

```csharp
var response = await client.DeleteAsync(recordId, "Invoices");
```

### Delete with Scripts

Use `SendAsync` with an `IDeleteRequest` to run scripts alongside a delete:

```csharp
var req = client.GenerateDeleteRequest();
req.Layout = "Invoices";
req.RecordId = recordId;
req.Script = "AfterDelete";
req.ScriptParameter = "param";
req.PreRequestScript = "BeforeDelete";
req.PreRequestScriptParameter = "preParam";

var response = await client.SendAsync(req);
```

### Reading Script Results

The `IDeleteResponse` contains an `ActionResponse` with script results:

```csharp
var scriptResult = response.Response.ScriptResult;
var scriptError = response.Response.ScriptError;  // 0 = no error

var preReqResult = response.Response.ScriptResultPreRequest;
var preSortResult = response.Response.ScriptResultPreSort;
```
