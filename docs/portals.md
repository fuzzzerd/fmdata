---
layout: default
parent: Guide
nav_order: 8
title: Portal Data
---

## Portal Data

Portals expose related records from a FileMaker layout. FMData maps portal data onto collection properties in your model.

### Model Setup

Use `[PortalData]` on an `IEnumerable<T>` property to map a portal:

```csharp
[DataContract(Name = "Invoices")]
public class Invoice
{
    [DataMember]
    public string InvoiceNumber { get; set; }

    [PortalData("InvoiceLineItems")]
    public IEnumerable<LineItem> LineItems { get; set; }
}

[DataContract(Name = "LineItems")]
public class LineItem
{
    [DataMember]
    public string Description { get; set; }

    [DataMember]
    public decimal Amount { get; set; }
}
```

### Table Prefix and SkipPrefix

FileMaker's Data API prefixes portal field names with the table occurrence name (e.g., `LineItems::Description`). FMData handles this automatically using the `TablePrefixFieldNames` property:

```csharp
// Use a custom prefix
[PortalData("InvoiceLineItems", TablePrefixFieldNames = "LineItems")]
public IEnumerable<LineItem> LineItems { get; set; }

// Skip the prefix entirely (fields come back without table:: prefix)
[PortalData("InvoiceLineItems", SkipPrefix = true)]
public IEnumerable<LineItem> LineItems { get; set; }
```

### Including Portals in Requests

#### Include Specific Portals

Use `IncludePortals` to specify which portals to return:

```csharp
var req = client.GenerateFindRequest<Invoice>();
req.Layout = "Invoices";
req.AddQuery(new Invoice { InvoiceNumber = "INV-001" }, omit: false);
req.IncludePortals("InvoiceLineItems", "Payments");

var results = await client.SendAsync(req);
```

#### ConfigurePortal

Use `ConfigurePortal` to set per-portal limit and offset:

```csharp
req.ConfigurePortal("InvoiceLineItems", limit: 100, offset: 1);
```

#### Fluent WithPortal Builder

Chain portal configuration using the fluent API:

```csharp
req.WithPortal("InvoiceLineItems").Limit(100).Offset(1)
   .WithPortal("Payments").Limit(50);
```

### Portal Limit and Offset

By default, the FileMaker Data API limits portal records to 50 per request. Use `limit` and `offset` to control pagination:

- **limit** — Maximum number of portal records to return
- **offset** — Starting position (1-based) in the portal's record set

```csharp
var req = client.GenerateFindRequest<Invoice>();
req.Layout = "Invoices";
req.AddQuery(new Invoice { InvoiceNumber = "INV-001" }, omit: false);

req.WithPortal("InvoiceLineItems").Limit(200).Offset(1)
   .WithPortal("Payments").Limit(100);

var results = await client.SendAsync(req);
```

Portal parameters work with both find requests (POST to `_find`) and get-records requests (GET).
