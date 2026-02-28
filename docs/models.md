---
layout: default
parent: Guide
nav_order: 1
title: Models & Mapping
---

## Models & Mapping

FMData uses `System.Runtime.Serialization` attributes to map C# classes to FileMaker layouts and fields.

### Layout Mapping

Use `[DataContract]` on your class with `Name` set to the FileMaker layout name:

```csharp
[DataContract(Name = "Invoices")]
public class Invoice
{
    [DataMember]
    public string InvoiceNumber { get; set; }

    [DataMember]
    public decimal Amount { get; set; }
}
```

### Field Name Overrides

When your C# property name doesn't match the FileMaker field name, use `DataMember(Name = "...")`:

```csharp
[DataMember(Name = "Invoice_Number")]
public string InvoiceNumber { get; set; }
```

### Ignoring Properties

Use `[IgnoreDataMember]` to exclude properties from serialization:

```csharp
[IgnoreDataMember]
public string ComputedValue { get; set; }
```

### Container Fields

Mark a `byte[]` property with `[ContainerDataFor]` to associate it with a container field:

```csharp
[DataMember]
public string Photo { get; set; }

[ContainerDataFor("Photo")]  // references the C# property name
public byte[] PhotoData { get; set; }
```

See [Container Data](containers.html) for loading and uploading container data.

### Portal Data

Use `[PortalData]` on a collection property to map related records from a portal:

```csharp
[PortalData("InvoiceLineItems")]
public IEnumerable<LineItem> LineItems { get; set; }
```

By default, FMData prefixes portal field names with the table occurrence name. Set `TablePrefixFieldNames` to control the prefix, or `SkipPrefix = true` to disable it:

```csharp
[PortalData("InvoiceLineItems", TablePrefixFieldNames = "LineItems", SkipPrefix = false)]
public IEnumerable<LineItem> LineItems { get; set; }
```

See [Portal Data](portals.html) for configuring portal requests.

### FileMaker Record ID Mapping

FileMaker assigns each record an internal record ID and modification ID. These aren't part of field data, so they're mapped via `Func` delegates at query time rather than attributes:

```csharp
[IgnoreDataMember]
public int FileMakerRecordId { get; set; }

[IgnoreDataMember]
public int FileMakerModId { get; set; }
```

```csharp
Func<Invoice, int, object> fmIdFunc = (o, id) => o.FileMakerRecordId = id;
Func<Invoice, int, object> fmModIdFunc = (o, id) => o.FileMakerModId = id;

var results = await client.FindAsync(query, fmIdFunc);
```

Alternatively, add a calculated field `Get(RecordID)` to your FileMaker layout and map it like any other field.
