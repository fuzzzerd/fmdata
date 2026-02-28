---
layout: default
nav_order: 3
title: Getting Started
---

## Getting Started

This walkthrough takes you from zero to working code with FMData.

### 1. Install the Package

```sh
dotnet add package FMData.Rest
```

### 2. Define a Model

Models map to FileMaker layouts using `DataContract` and `DataMember` attributes from `System.Runtime.Serialization`.

```csharp
using System.Runtime.Serialization;

[DataContract(Name = "Contacts")]  // FileMaker layout name
public class Contact
{
    [DataMember]
    public string FirstName { get; set; }

    [DataMember]
    public string LastName { get; set; }

    [DataMember(Name = "Email_Address")]  // map to a differently-named field
    public string Email { get; set; }

    [IgnoreDataMember]  // not sent to FileMaker
    public int FileMakerRecordId { get; set; }
}
```

### 3. Create a Client

```csharp
using FMData;
using FMData.Rest;

var client = new FileMakerRestClient(new HttpClient(), new ConnectionInfo
{
    FmsUri = "https://your-server.com",
    Database = "YourDatabase",
    Username = "admin",
    Password = "password"
});
```

### 4. Find Records

Set properties on a model instance to define your search criteria, then call `FindAsync`:

```csharp
var query = new Contact { LastName = "Smith" };
var results = await client.FindAsync(query);

foreach (var contact in results)
{
    Console.WriteLine($"{contact.FirstName} {contact.LastName} - {contact.Email}");
}
```

### 5. Create a Record

```csharp
var newContact = new Contact
{
    FirstName = "Jane",
    LastName = "Doe",
    Email = "jane@example.com"
};

var response = await client.CreateAsync(newContact);
// response.Messages contains the status code and message
```

### 6. Edit a Record

```csharp
var recordId = 42;  // the FileMaker record ID
var updated = new Contact { Email = "newemail@example.com" };
var response = await client.EditAsync(recordId, updated);
```

### 7. Delete a Record

```csharp
await client.DeleteAsync("Contacts", recordId);
```

### Next Steps

- [Guide](guide.html) — Detailed coverage of every operation
- [Configuration](configuration.html) — Dependency injection, authentication, and client lifetime
- [Error Handling](error-handling.html) — Working with FileMaker error codes
