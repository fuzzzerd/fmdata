---
layout: default
parent: Guide
nav_order: 4
title: Editing Records
---

## Editing Records

### Basic Edit

Pass the FileMaker record ID and a model with the fields to update:

```csharp
var recordId = 42;
var updated = new Invoice { Status = "Closed", Amount = 200.00m };
var response = await client.EditAsync(recordId, updated);
```

### Layout Override

Specify a layout explicitly:

```csharp
var response = await client.EditAsync("AlternateLayout", recordId, updated);
```

### Null and Default Value Control

By default, null and default-valued properties are excluded. To include them:

```csharp
var response = await client.EditAsync(recordId, updated,
    includeNullValues: true,
    includeDefaultValues: true);
```

### Edit with Scripts

Run FileMaker scripts alongside the edit:

```csharp
var response = await client.EditAsync(recordId,
    script: "AfterEdit", scriptParameter: "param", input: updated);
```

### Dictionary-Based Edit

Edit specific fields without a model class:

```csharp
var fields = new Dictionary<string, string>
{
    { "Status", "Closed" },
    { "Notes", "Updated via API" }
};

var response = await client.EditAsync(recordId, "Invoices", fields);
```

### Reading Script Results

The `IEditResponse` contains an `ActionResponse` with script results:

```csharp
var response = await client.EditAsync(recordId,
    script: "AfterEdit", scriptParameter: "param", input: updated);

var scriptResult = response.Response.ScriptResult;
var scriptError = response.Response.ScriptError;  // 0 = no error

var preReqResult = response.Response.ScriptResultPreRequest;
var preSortResult = response.Response.ScriptResultPreSort;
```

### Advanced: SendAsync with IEditRequest

For full control, build a request manually:

```csharp
var req = client.GenerateEditRequest(updated);
req.Layout = "Invoices";
req.RecordId = recordId;
req.Script = "AfterEdit";
req.ScriptParameter = "param";

var response = await client.SendAsync(req);
```
