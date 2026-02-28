---
layout: default
parent: Guide
nav_order: 6
title: Running Scripts
---

## Running Scripts

FileMaker scripts can be executed standalone or attached to any CRUD operation.

### Script Execution Order

When scripts are attached to a request, FileMaker executes them in this order:

1. **Pre-request script** — runs before the operation
2. **Pre-sort script** — runs after the operation but before sorting
3. **Post-request script** — runs after sorting

### Standalone Script Execution

Run a script directly with `RunScriptAsync`:

```csharp
var result = await client.RunScriptAsync("Invoices", "CleanupScript", "parameter");
// result is the script result string, or the script error code as a string
```

The layout parameter specifies the context layout for the script. The method returns the script result as a string. If the script produces an error, the error code is returned as a string.

### Attaching Scripts to Requests

Every request type supports three script slots via properties on `IFileMakerRequest`:

| Property | Parameter Property | Execution Phase |
|---|---|---|
| `Script` | `ScriptParameter` | After the operation and sort |
| `PreRequestScript` | `PreRequestScriptParameter` | Before the operation |
| `PreSortScript` | `PreSortScriptParameter` | After the operation, before sort |

#### Create with Scripts

```csharp
var response = await client.CreateAsync(invoice,
    script: "AfterCreate", scriptParameter: "param",
    preRequestScript: "BeforeCreate", preRequestScriptParam: "preParam",
    preSortScript: "SortSetup", preSortScriptParameter: "sortParam");
```

#### Edit with Scripts

```csharp
var response = await client.EditAsync(recordId,
    script: "AfterEdit", scriptParameter: "param", input: invoice);
```

#### Delete with Scripts

```csharp
var req = client.GenerateDeleteRequest();
req.Layout = "Invoices";
req.RecordId = recordId;
req.Script = "AfterDelete";
req.ScriptParameter = "param";
var response = await client.SendAsync(req);
```

#### Find with Scripts

```csharp
var results = await client.FindAsync(query, skip: 0, take: 100,
    script: "AfterFind", scriptParameter: "param",
    fmIdFunc: fmIdFunc);
```

### Reading Script Results

Script results are returned via `ActionResponse`, which contains:

```csharp
int ScriptError              // 0 = success
string ScriptResult          // post-request script result
int ScriptErrorPreRequest    // 0 = success
string ScriptResultPreRequest
int ScriptErrorPreSort       // 0 = success
string ScriptResultPreSort
```

#### From Create, Edit, and Delete

These operations return `ICreateResponse`, `IEditResponse`, or `IDeleteResponse`, each with a `Response` property:

```csharp
var response = await client.CreateAsync(invoice,
    script: "MyScript", scriptParameter: "param");

var result = response.Response.ScriptResult;
var error = response.Response.ScriptError;
```

#### From Find (SendAsync with DataInfo)

Use the `includeDataInfo` overload to get script results from find operations:

```csharp
var req = client.GenerateFindRequest<Invoice>();
req.Layout = "Invoices";
req.AddQuery(new Invoice { Status = "Open" }, omit: false);
req.Script = "AfterFind";
req.ScriptParameter = "param";

var (data, dataInfo, scriptResponse) = await client.SendAsync(req, includeDataInfo: true);

var result = scriptResponse?.ScriptResult;
var error = scriptResponse?.ScriptError;
```

### Script Error Handling

A non-zero `ScriptError` indicates the script encountered an error. Check the error code after any operation that runs scripts:

```csharp
if (response.Response.ScriptError != 0)
{
    Console.WriteLine($"Script error {response.Response.ScriptError}");
}
```

FileMaker script error codes are the same codes returned by `Get(LastError)` in FileMaker Pro.
