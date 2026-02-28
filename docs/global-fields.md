---
layout: default
parent: Guide
nav_order: 9
title: Global Fields
---

## Global Fields

FileMaker global fields store values that are visible across the current session. FMData provides `SetGlobalFieldAsync` to set these values.

### Setting a Global Field

```csharp
var response = await client.SetGlobalFieldAsync(
    "BaseTable",        // the base table name
    "GlobalStatus",     // the global field name
    "Active");          // the value to set
```

The field name should be the fully qualified field name as FileMaker expects it (table occurrence and field name).

> **Note (XML client):** When using the XML client, global field values are queued and sent with the next request to the server rather than being sent immediately.
