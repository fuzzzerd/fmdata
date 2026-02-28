---
layout: default
parent: Guide
nav_order: 3
title: Creating Records
---

## Creating Records

### Basic Create

Pass a model instance to `CreateAsync`. The layout is determined by the `[DataContract(Name = "...")]` attribute:

```csharp
var invoice = new Invoice
{
    InvoiceNumber = "INV-001",
    Amount = 150.00m,
    Status = "Open"
};

var response = await client.CreateAsync(invoice);
```

### Layout Override

Specify a layout explicitly:

```csharp
var response = await client.CreateAsync("AlternateLayout", invoice);
```

### Null and Default Value Control

By default, properties with `null` or default values are excluded from the request. To include them:

```csharp
var response = await client.CreateAsync(invoice,
    includeNullValues: true,
    includeDefaultValues: true);
```

### Create with Scripts

Run FileMaker scripts as part of the create operation. Scripts execute in this order: pre-request, pre-sort, then post-request (after the record is created).

```csharp
var response = await client.CreateAsync(invoice,
    script: "AfterCreate", scriptParameter: "param1",
    preRequestScript: "BeforeCreate", preRequestScriptParam: "param2",
    preSortScript: "SortSetup", preSortScriptParameter: "param3");
```

### Reading Script Results

The `ICreateResponse` contains an `ActionResponse` with script results:

```csharp
var response = await client.CreateAsync(invoice,
    script: "AfterCreate", scriptParameter: "param");

var scriptResult = response.Response.ScriptResult;
var scriptError = response.Response.ScriptError;  // 0 = no error

var preReqResult = response.Response.ScriptResultPreRequest;
var preSortResult = response.Response.ScriptResultPreSort;
```

### Advanced: SendAsync with ICreateRequest

For full control, build a request manually:

```csharp
var req = client.GenerateCreateRequest(invoice);
req.Layout = "Invoices";
req.Script = "AfterCreate";
req.ScriptParameter = "param";

var response = await client.SendAsync(req);
```
