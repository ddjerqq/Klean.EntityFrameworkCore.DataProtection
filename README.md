# Klean.EntityFrameworkCore.DataProtection

[![tests](https://github.com/ddjerqq/Klean.EntityFrameworkCore.DataProtection/actions/workflows/build.yaml/badge.svg)](https://github.com/ddjerqq/Klean.EntityFrameworkCore.DataProtection/actions/workflows/test.yml)
[![Nuget](https://img.shields.io/nuget/v/Klean.EntityFrameworkCore.DataProtection.svg)](https://www.nuget.org/packages/Klean.EntityFrameworkCore.DataProtection)
[![Nuget Downloads](https://img.shields.io/nuget/dt/Klean.EntityFrameworkCore.DataProtection)](https://www.nuget.org/packages/Klean.EntityFrameworkCore.DataProtection)

`Klean.EntityFrameworkCore.DataProtection` is a [Microsoft Entity Framework Core](https://github.com/aspnet/EntityFrameworkCore) extension which
adds support for data protection and querying for encrypted properties for your entities.

# What problem does this library solve?

When you need to store sensitive data in your database, you may want to encrypt it to protect it from unauthorized access, however, when you
encrypt data, it becomes impossible to query it by EF-core, which is not really convenient if you want to encrypt, for example, email addresses, or SSNs
AND then filter entities by them.

This library has support for hashing the sensitive data and storing their (sha256) hashes in a shadow property alongside the encrypted data.
This allows you to query for the encrypted properties without decrypting them first. using `QueryableExt.WherePdEquals`

# Disclaimer

This project is maintained by [one (10x) developer](https://github.com/ddjerqq) and is not affiliated with Microsoft.

I made this library to solve my own problems with EFCore. I needed to store a bunch of protected personal data encrypted, among these properties were personal IDs, Emails, SocialSecurityNumbers and so on.
As you know, you cannot query encrypted data with EFCore, and I wanted a simple yet boilerplate-free solution. Thus, I made this library.

**What this library allows you to do, is to encrypt your properties and query them without decrypting them first. It does so by hashing the encrypted data and storing the hash in a shadow property alongside the encrypted data.**

I **do not** take responsibility for any damage done in production environments and lose of your encryption key or corruption of your data.

Keeping your encryption keys secure is your responsibility. If you lose your encryption key, **you will lose your data.**

## Currently supported property types

- string
- byte[]

# Getting started

### Installing the package

Install the package from [NuGet](https://www.nuget.org/) or from the `Package Manager Console` :

```powershell
PM> Install-Package Klean.EntityFrameworkCore.DataProtection
```

### Configuring Data Protection in your DbContext

`YourDbContext.cs`

```csharp
public class Your(DbContextOptions<Your> options, IDataProtectionProvider dataProtectionProvider) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.UseDataProtection(dataProtectionProvider);
    }
}
```

> [!WARNING]
> The call to `builder.UseDataProtection` **MUST** come after the call to `base.OnModelCreating` in your `DbContext` class
> and before any other configuration you might have.

### Registering the services

`Program.cs`

```csharp
builder.Services.AddDataProtectionServices();
```

To persist keys in file system use the following code:

```csharp
var keyDirectory = new DirectoryInfo("path/to/solution/.aspnet/dp/keys");
builder.Services.AddDataProtectionServices()
    .PersistKeysToFileSystem(keyDirectory);
```

> [!TIP]
> See the [Microsoft documentation](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview) for more
> information on **how to configure the data protection** services, and how to store your encryption keys securely.

### Configure the data protection options on your DbContext

`Program.cs`
```csharp
services.AddDbContext<YourDbContext>(opt => opt
    .AddDataProtectionInterceptors()
    /* ... */);
```

> [!WARNING]
> You **MUST** call `AddDataProtectionInterceptors` if you are using any encrypted properties that are queryable in your entities.
> If you are not using any queryable encrypted properties, you can skip this step.

## Usage:

### Marking your properties as encrypted

There are three ways you can mark your properties as encrypted:

Using the EncryptedAttribute:
```csharp
class User
{
  [Encrypt(IsQueryable = true, IsUnique = true)]
  public string SocialSecurityNumber { get; set; }

  [Encrypt(IsQueryable = false, IsUnique = false)]
  public byte[] IdPicture { get; set; }
}
```

> [!TIP]
> By default `IsQueryable` and `IsUnique` are set to `true`, you can omit them if you want to use the default values.
> If you have a property that is marked as `IsQueryable = true` and you want to query it, you **MUST** call `AddDataProtectionInterceptors` in your `DbContext` configuration.

Using the FluentApi (in your `DbContext.OnModelCreating` method):
```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
  builder.Entity<User>(entity =>
  {
    //                                                                v defaults to true
    entity.Property(e => e.SocialSecurityNumber).IsEncryptedQueryable(isUnique: true);
    entity.Property(e => e.IdPicture).IsEncrypted();
  });
}
```

The above step also applies to custom `EntityTypeConfiguration`s

### Querying encrypted properties

You can query encrypted properties that are marked as Queryable using the `IQueryable<T>.WherePdEquals` extension method:

```csharp
var foo = await DbContext.Users
  .WherePdEquals(nameof(User.SocialSecurityNumber), "404-69-1337")
  .SingleOrDefaultAsync();
```

> [!WARNING]
> The `QueryableExt.WherePdEquals` method is only available for properties that are marked as Queryable using the `[Encrypt(IsQueryable = true)]` attribute or the
> `IsEncryptedQueryable()` method.

> [!CAUTION]
> Before using `WherePdEquals` you **MUST** call `AddDataProtectionInterceptors` in your `DbContext` configuration.
> There will be no error if you forget to call `AddDataProtectionInterceptors`, but the query will not work as expected.

> [!NOTE]
> The `WherePdEquals` extension method generates an expression like this one under the hood:<br/>
> `Where(e => EF.Property<string>(e, $"{propertyName}ShadowHash") == value.Sha256Hash())`

### Profit!

## Thank you for using this library!

ddjerqq <3