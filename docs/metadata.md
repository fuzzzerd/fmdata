---
layout: default
parent: Guide
nav_order: 10
title: Server Metadata
---

## Server Metadata

FMData can query FileMaker Server for information about the server, databases, layouts, scripts, and field metadata.

### Product Information

```csharp
var info = await client.GetProductInformationAsync();

Console.WriteLine(info.Name);          // e.g., "FileMaker"
Console.WriteLine(info.Version);       // System.Version
Console.WriteLine(info.BuildDate);     // DateTime
Console.WriteLine(info.DateFormat);    // e.g., "MM/dd/yyyy"
Console.WriteLine(info.TimeFormat);    // e.g., "HH:mm:ss"
Console.WriteLine(info.TimeStampFormat);
```

### List Databases

```csharp
IReadOnlyCollection<string> databases = await client.GetDatabasesAsync();

foreach (var db in databases)
{
    Console.WriteLine(db);
}
```

### List Layouts

```csharp
IReadOnlyCollection<LayoutListItem> layouts = await client.GetLayoutsAsync();

foreach (var layout in layouts)
{
    Console.WriteLine(layout.Name);

    if (layout.IsFolder)
    {
        foreach (var child in layout.FolderLayoutNames)
        {
            Console.WriteLine($"  {child.Name}");
        }
    }
}
```

### Get Layout Metadata

Retrieve detailed field and value list information for a specific layout:

```csharp
LayoutMetadata meta = await client.GetLayoutAsync("Invoices");

// Field metadata
foreach (var field in meta.FieldMetaData)
{
    Console.WriteLine($"{field.Name}: {field.Result} ({field.Type})");
    Console.WriteLine($"  AutoEnter: {field.AutoEnter}, NotEmpty: {field.NotEmpty}");
    Console.WriteLine($"  Global: {field.Global}, MaxRepeat: {field.MaxRepeat}");
}

// Value lists
foreach (var vl in meta.ValueLists)
{
    Console.WriteLine($"Value list: {vl.Name} ({vl.Type})");
    foreach (var item in vl.Values)
    {
        Console.WriteLine($"  {item.DisplayValue} = {item.Value}");
    }
}
```

You can optionally pass a record ID to get value lists that depend on a specific record's context:

```csharp
var meta = await client.GetLayoutAsync("Invoices", recordId: 42);
```

### List Scripts

```csharp
IReadOnlyCollection<ScriptListItem> scripts = await client.GetScriptsAsync();

foreach (var script in scripts)
{
    Console.WriteLine(script.Name);

    if (script.IsFolder)
    {
        foreach (var child in script.FolderScriptNames)
        {
            Console.WriteLine($"  {child.Name}");
        }
    }
}
```
