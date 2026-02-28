---
layout: default
nav_order: 5
title: Configuration
---

## Configuration

### ConnectionInfo

All clients require a `ConnectionInfo` object:

```csharp
var conn = new ConnectionInfo
{
    FmsUri = "https://your-server.com",
    Database = "YourDatabase",
    Username = "admin",
    Password = "password"
};
```

#### REST API Version

Set `RestTargetVersion` to target a specific Data API version:

```csharp
conn.RestTargetVersion = RestTargetVersion.v1;      // default
conn.RestTargetVersion = RestTargetVersion.v2;
conn.RestTargetVersion = RestTargetVersion.vLatest;
```

### Creating a Client Directly

```csharp
var client = new FileMakerRestClient(new HttpClient(), conn);
```

Or using `IHttpClientFactory` (available on netstandard2.0+):

```csharp
var client = new FileMakerRestClient(httpClientFactory, conn);
```

When constructed with `IHttpClientFactory`, the client uses factory-managed clients for container downloads, avoiding connection pool exhaustion from creating ad-hoc `HttpClient` instances.

### Using Dependency Injection

#### Using Extension Methods (Recommended)

The simplest approach uses the built-in `AddFMDataRest` extension method, which handles all registration in one call:

```csharp
services.AddFMDataRest(conn =>
{
    conn.FmsUri = "https://your-server.com";
    conn.Database = "YourDatabase";
    conn.Username = "admin";
    conn.Password = "password";
});
```

This registers:

- `ConnectionInfo` as a singleton
- `IAuthTokenProvider` (using `DefaultAuthTokenProvider`)
- A named `HttpClient` via `IHttpClientFactory`
- `FileMakerRestClient` as a singleton, available as both `IFileMakerApiClient` and `IFileMakerRestClient`

You can optionally configure the underlying `HttpClient`:

```csharp
services.AddFMDataRest(
    conn =>
    {
        conn.FmsUri = "https://your-server.com";
        conn.Database = "YourDatabase";
        conn.Username = "admin";
        conn.Password = "password";
    },
    httpClient =>
    {
        httpClient.Timeout = TimeSpan.FromSeconds(30);
    });
```

The method returns `IHttpClientBuilder`, which lets you chain additional configuration such as retry policies or custom message handlers.

For the XML/CWP client, use `AddFMDataXml` from the `FMData.Xml` namespace:

```csharp
services.AddFMDataXml(conn =>
{
    conn.FmsUri = "https://your-server.com";
    conn.Database = "YourDatabase";
    conn.Username = "admin";
    conn.Password = "password";
});
```

> **Note:** The `AddFMDataRest` and `AddFMDataXml` extension methods require netstandard2.0 or later.

#### Manual Registration

If you prefer manual control or are targeting netstandard1.3/net45, you can wire up registration yourself.

**Standard (Scoped) Lifetime:**

```csharp
services.AddSingleton<ConnectionInfo>(new ConnectionInfo
{
    FmsUri = "https://your-server.com",
    Database = "YourDatabase",
    Username = "admin",
    Password = "password"
});

services.AddHttpClient<IFileMakerApiClient, FileMakerRestClient>();
```

This creates a new `FileMakerRestClient` per scope (typically per HTTP request in ASP.NET Core).

**Singleton Lifetime:**

For better performance when making many Data API calls per request, register as a singleton:

```csharp
services.AddHttpClient();  // register IHttpClientFactory

services.AddSingleton<ConnectionInfo>(new ConnectionInfo
{
    FmsUri = "https://your-server.com",
    Database = "YourDatabase",
    Username = "admin",
    Password = "password"
});

services.AddSingleton<IFileMakerApiClient, FileMakerRestClient>(sp =>
{
    var hcf = sp.GetRequiredService<IHttpClientFactory>();
    var ci = sp.GetRequiredService<ConnectionInfo>();
    return new FileMakerRestClient(hcf.CreateClient(), ci);
});
```

The singleton approach reuses the FileMaker Data API token across requests, reducing authentication overhead.

### FileMaker Cloud Authentication

For FileMaker Cloud (Claris Connect), use `FileMakerCloudAuthTokenProvider`:

```csharp
var conn = new ConnectionInfo
{
    FmsUri = "https://yourhost.account.filemaker-cloud.com",
    Database = "YourDatabase",
    Username = "user@domain.com",
    Password = "password"
};

var client = new FileMakerRestClient(
    new HttpClient(),
    new FileMakerCloudAuthTokenProvider(conn));
```

`FileMakerCloudAuthTokenProvider` handles authentication via AWS Cognito. The default Cognito settings are pre-configured for FileMaker Cloud:

| Property | Default |
|---|---|
| `CognitoUserPoolID` | `us-west-2_NqkuZcXQY` |
| `CognitoClientID` | `4l9rvl4mv5es1eep1qe97cautn` |
| `RegionEndpoint` | `us-west-2` |

Override these on `ConnectionInfo` if your FileMaker Cloud instance uses different values.

For a full description of using the FileMaker Data API with FileMaker Cloud, see [this discussion](https://github.com/fuzzzerd/fmdata/issues/217#issuecomment-1203202293).
