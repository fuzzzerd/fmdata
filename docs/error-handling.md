---
layout: default
nav_order: 6
title: Error Handling
---

## Error Handling

### FMDataException

When the FileMaker Data API returns an error, FMData throws an `FMDataException`:

```csharp
try
{
    var results = await client.FindAsync(query);
}
catch (FMDataException ex)
{
    Console.WriteLine($"FileMaker error {ex.FMErrorCode}: {ex.FMErrorMessage}");
}
```

`FMDataException` extends `System.Exception` and adds:

| Property | Type | Description |
|---|---|---|
| `FMErrorCode` | `int` | The FileMaker error code |
| `FMErrorMessage` | `string` | Human-readable description of the error |

### Common Error Codes

| Code | Meaning |
|---|---|
| 0 | No error (success) |
| 102 | Field is missing |
| 105 | Layout is missing |
| 106 | Table is missing |
| 401 | No records match the request |
| 500 | Date value does not meet validation |
| 802 | Unable to open the file |
| 952 | Invalid FileMaker Data API token |

FMData includes a comprehensive dictionary of all FileMaker error codes (0–1715) and their descriptions.

### How FMData Maps HTTP Responses

The FileMaker Data API returns standard HTTP status codes, but uses `500 Internal Server Error` for many application-level errors. FMData handles these cases:

#### Find Operations

| HTTP Status | FileMaker Code | FMData Behavior |
|---|---|---|
| 200 OK | — | Returns results normally |
| 404 Not Found | — | Returns an empty collection |
| 500 Internal Server Error | 401 | Returns an empty collection (no records found) |
| 500 Internal Server Error | Other | Throws `FMDataException` with code and message |

#### GetByFileMakerIdAsync

| HTTP Status | FileMaker Code | FMData Behavior |
|---|---|---|
| 200 OK | — | Returns the record |
| 404 Not Found | — | Returns `null` |
| 500 Internal Server Error | 401 | Returns `null` |
| 500 Internal Server Error | Other | Throws `FMDataException` with code and message |

#### Create and Edit Operations

| HTTP Status | FMData Behavior |
|---|---|
| 200 OK | Returns the response |
| 404 Not Found | Returns response with `"404"` error code (edit only) |
| 500 Internal Server Error | Throws `FMDataException` with code and message |

#### Delete Operations

| HTTP Status | FMData Behavior |
|---|---|
| 200 OK | Returns the response |
| 404 Not Found | Returns response with `"404"` code and `"Error"` message |
| 500 Internal Server Error | Throws `FMDataException` with code and message |

### Authentication Errors

FMData automatically retries requests that receive a `401 Unauthorized` HTTP response by refreshing the authentication token. This handles expired Data API tokens transparently.
