# SilverCodeAPI

[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/badge/NuGet-available-blue)](https://github.com/BigBadJock/SilverCodeAPI/packages)

A set of .NET 10 NuGet packages providing interfaces and abstract base classes for building API services using the **Repository** and **Data Service** patterns on top of Entity Framework Core, with built-in URL-driven filtering, sorting and pagination via [REST-Parser](https://github.com/BigBadJock/REST-Parser).

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Repositories](#repositories)
- [Data Services](#data-services)
- [Unit of Work](#unit-of-work)
- [Data Models](#data-models)
- [REST Query Syntax](#rest-query-syntax)
- [Auditing](#auditing)
- [Best Practices](#best-practices)
- [Known Limitations](#known-limitations)
- [Troubleshooting](#troubleshooting)
- [Additional Resources](#additional-resources)

---

## Overview

- **Generic Repository Pattern** — CRUD operations over an EF Core `DbSet<T>`
- **Data Service Layer** — a logging/business-logic layer over repositories
- **REST Query Support** — URL-driven filtering, sorting and pagination via REST-Parser
- **Multiple ID Types** — `int`, `Guid` and `string` primary keys
- **Audit Fields** — `Created`, `CreatedBy`, `LastUpdated`, `LastUpdatedBy` and `IsDeleted` on every entity
- **Read-Only Repositories** — separate interfaces and base classes for read-only access
- **Auth DTOs** — `Credentials`, `RefreshTokenCredentials`, `JWTSettings`, `BaseUser` and `IBaseTokenService`

### Packages

| Package | Description | Depends on |
|---------|-------------|------------|
| `Core.Common.DataModels` | Base entity models, DTOs and model interfaces | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` |
| `Core.Common.Contracts` | Repository, data service, auditor, unit-of-work and token service interfaces | `Core.Common.DataModels`, `Microsoft.EntityFrameworkCore`, `REST-Parser` |
| `Core.Common` | Abstract base implementations of the repositories, data services and auditor | `Core.Common.Contracts`, `Ardalis.GuardClauses` |

---

## Architecture

```
┌─────────────────────────────────────────────────────┐
│              Your API Controller                    │
└──────────────────┬──────────────────────────────────┘
                   ▼
┌─────────────────────────────────────────────────────┐
│  Data Service Layer                                 │
│  (BaseDataServiceWithIntId, ...WithGuidId, ...)     │
└──────────────────┬──────────────────────────────────┘
                   ▼
┌─────────────────────────────────────────────────────┐
│  Repository Layer                                   │
│  (BaseRepositoryWithIntId, ...WithGuidId, ...)      │
└──────────────────┬──────────────────────────────────┘
                   ▼
┌─────────────────────────────────────────────────────┐
│  EF Core DbContext (one per repository instance,    │
│  created from IDbContextFactory<DBC>)               │
└─────────────────────────────────────────────────────┘
```

### Type map

```
Core.Common.Contracts             Core.Common                     Core.Common.DataModels
─────────────────────             ───────────                     ──────────────────────
IReadRepository<DBC,T>        ←─  BaseReadRepository              BaseModel
IReadRepositoryWithIntId      ←─  BaseReadRepositoryWithIntId     BaseModelWithIntId
IReadRepositoryWithGuidId     ←─  BaseReadRepositoryWithGuidId    BaseModelWithGuidId
IReadRepositoryWithStringId   ←─  BaseReadRepositoryWithStringId  BaseModelWithStringId
                                                                  BaseLookupModel
IRepository<DBC,T>            ←─  BaseRepository                  BaseUser
IRepositoryWithIntId          ←─  BaseRepositoryWithIntId
IRepositoryWithGuidId         ←─  BaseRepositoryWithGuidId        ApiResult<T>
IRepositoryWithStringId       ←─  BaseRepositoryWithStringId      Pagination
                                                                  ProgressReport
IDataService<DBC,T>           ←─  BaseDataService                 Credentials
IDataServiceWithIntId         ←─  BaseDataServiceWithIntId        RefreshTokenCredentials
IDataServiceWithGuidId        ←─  BaseDataServiceWithGuidId       JWTSettings
IDataServiceWithStringId      ←─  BaseDataServiceWithStringId

IAuditor                      ←─  BaseAuditor
IUnitOfWork                       (no base implementation)
IBaseTokenService<DBC,T>          (no base implementation)
                                  Helpers.ObjectExtensions
```

---

## Installation

### .NET CLI

```bash
dotnet add package Core.Common.Contracts
dotnet add package Core.Common.DataModels
dotnet add package Core.Common
```

`REST-Parser` is brought in transitively by `Core.Common.Contracts`; add it explicitly only if you need a different version.

### Package References

```xml
<ItemGroup>
  <PackageReference Include="Core.Common.Contracts" Version="1.2026.*" />
  <PackageReference Include="Core.Common.DataModels" Version="1.2026.*" />
  <PackageReference Include="Core.Common" Version="1.2026.*" />
</ItemGroup>
```

Package versions are generated at build time in the form `1.yyyy.Mdd.Hmm`.

### GitHub Packages

1. Create a personal access token with `read:packages` scope (**GitHub → Settings → Developer Settings → Personal Access Tokens**).
2. Add a `nuget.config` to your solution root. Prefer environment variables over a hard-coded token:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/bigbadjock/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="%GITHUB_PACKAGES_USER%" />
      <add key="ClearTextPassword" value="%GITHUB_PACKAGES_PAT%" />
    </github>
  </packageSourceCredentials>
</configuration>
```

If you do put a token directly in `nuget.config`, add the file to `.gitignore`.

---

## Quick Start

### 1. Define your entities

Inherit from the base model matching your ID type:

```csharp
using Core.Common.DataModels;

public class Product : BaseModelWithIntId
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class Order : BaseModelWithGuidId
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class UserProfile : BaseModelWithStringId
{
    public string Email { get; set; } = string.Empty;
}
```

### 2. Create your DbContext

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
}
```

### 3. Implement a repository

```csharp
using Core.Common;
using Core.Common.Contracts;
using REST_Parser;

public interface IProductRepository : IRepositoryWithIntId<AppDbContext, Product> { }

public class ProductRepository : BaseRepositoryWithIntId<AppDbContext, Product>, IProductRepository
{
    public ProductRepository(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IRestToLinqParser<Product> parser,
        ILogger<IRepository<AppDbContext, Product>> logger)
        : base(dbContextFactory, parser, logger) { }
}
```

### 4. Implement a data service

Take your own repository interface in the constructor — it derives from `IRepositoryWithIntId<,>`, so it can be passed straight to the base class, and it is the type you register in DI.

```csharp
public interface IProductService : IDataServiceWithIntId<AppDbContext, Product> { }

public class ProductService : BaseDataServiceWithIntId<AppDbContext, Product>, IProductService
{
    public ProductService(
        IProductRepository repository,
        ILogger<IDataServiceWithIntId<AppDbContext, Product>> logger)
        : base(repository, logger) { }
}
```

### 5. Register services

```csharp
// Program.cs
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.RegisterRestParser<Product>();
builder.Services.RegisterRestParser<Order>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
```

### 6. Use in a controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET api/products?q=category=Electronics%26price[lt]=1000%26$sort_by=price[ASC]%26$page=1%26$pagesize=20
    [HttpGet]
    public IActionResult Search([FromQuery] string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
            q = "$sort_by=Id&$page=1&$pagesize=20";

        return Ok(_productService.Search(q));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var product = await _productService.GetById(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        var created = await _productService.Add(product);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        if (id != product.Id) return BadRequest();
        return Ok(await _productService.Update(product));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => await _productService.Delete(id) ? NoContent() : NotFound();
}
```

> Tip: to accept the REST query as the raw request query string instead of a `q` parameter, pass `Request.QueryString.Value?.TrimStart('?')` to `Search`.

---

## Repositories

### ID type variants

| ID type | Model | Repository | Read-only repository | Data service |
|---------|-------|------------|----------------------|--------------|
| `int` | `BaseModelWithIntId` | `BaseRepositoryWithIntId<DBC,T>` | `BaseReadRepositoryWithIntId<DBC,T>` | `BaseDataServiceWithIntId<DBC,T>` |
| `Guid` | `BaseModelWithGuidId` | `BaseRepositoryWithGuidId<DBC,T>` | `BaseReadRepositoryWithGuidId<DBC,T>` | `BaseDataServiceWithGuidId<DBC,T>` |
| `string` | `BaseModelWithStringId` | `BaseRepositoryWithStringId<DBC,T>` | `BaseReadRepositoryWithStringId<DBC,T>` | `BaseDataServiceWithStringId<DBC,T>` |

Constructor logger types differ slightly: write repositories take `ILogger<IRepository<DBC,T>>`; read-only repositories take `ILogger<IReadRepositoryWithXxxId<DBC,T>>`.

### Members

```csharp
// IReadRepository<DBC,T>
DbSet<T>       DbSet { get; }
bool           AlwaysIncludeChildren { get; set; }
IQueryable<T>  GetAll();                         // deferred query over the DbSet
ApiResult<T>   GetAll(string restQuery);         // executes the query, returns data + pagination

// IReadRepositoryWithXxxId<DBC,T>
Task<T?>       GetById(TId id);                  // DbSet.FindAsync — null if not found

// IRepository<DBC,T>
Task<T>        Add(T entity, bool commit = true);
Task<T>        Update(T entity, bool commit = true);
Task<bool>     Delete(Expression<Func<T, bool>> where, bool commit = true);
Task           AddBatch(IEnumerable<T> entities, int batchSize, IProgress<ProgressReport> progress);
Task           Commit();                         // SaveChangesAsync on this repository's context

// IRepositoryWithXxxId<DBC,T>
Task<bool>     Delete(TId id, bool commit = true);
```

`BaseRepository` also exposes a public `virtual Task<bool> Delete(T entity, bool commit = true)` that is not on the interface.

### Behaviour notes

| Method | Behaviour |
|--------|-----------|
| `Add` | Sets `Created` and `LastUpdated` to `DateTime.UtcNow`. Throws on `null` or `DbUpdateException`. |
| `Update` | Attaches the entity and marks **all** properties modified. Does not touch any audit fields. |
| `Delete(id)` | Hard delete. Returns `false` if the entity is not found **or** a `DbUpdateException` occurs. |
| `Delete(entity)` | Hard delete. Returns `false` on `DbUpdateException`. |
| `Delete(where)` | Hard delete of all matches. Returns `true` even if nothing matched; rethrows `DbUpdateException`. |
| `AddBatch` | Adds with `commit: false`, calls `Commit()` roughly every `batchSize` entities and once at the end, reporting progress after each entity. `progress` must not be `null`. |
| `GetById` | Uses `FindAsync`; never applies `Include`s. |

### Examples

```csharp
IQueryable<Product> query = repository.GetAll();

ApiResult<Product> page = repository.GetAll("category=Electronics&price[lt]=1000&$page=1&$pagesize=20");

Product? product = await repository.GetById(5);

await repository.Add(product, commit: false);
await repository.Add(other, commit: false);
await repository.Commit();

var progress = new Progress<ProgressReport>(r =>
    Console.WriteLine($"{r.Message}: {r.CurrentProgress}/{r.TotalProgress}"));
await repository.AddBatch(products, batchSize: 100, progress);

bool deleted = await repository.Delete(5);
```

### Custom repository methods

The underlying `DbSet<T>` is available as the protected field `dbset` (and the public `DbSet` property):

```csharp
public interface IProductRepository : IRepositoryWithIntId<AppDbContext, Product>
{
    Task<List<Product>> GetLowStockProducts(int threshold);
}

public class ProductRepository : BaseRepositoryWithIntId<AppDbContext, Product>, IProductRepository
{
    public ProductRepository(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IRestToLinqParser<Product> parser,
        ILogger<IRepository<AppDbContext, Product>> logger)
        : base(dbContextFactory, parser, logger) { }

    public Task<List<Product>> GetLowStockProducts(int threshold)
        => dbset.Where(p => p.Stock < threshold && !p.IsDeleted).ToListAsync();
}
```

---

## Data Services

Data services wrap a repository and add entry/exit/error logging. All methods except `GetAll()` are `virtual`.

```csharp
// IDataService<DBC,T>
Task<T>        Add(T model);                     // repository.Add(model)  — commits
Task<T>        Update(T model);                  // repository.Update(model) — commits
Task<bool>     Delete(Expression<Func<T, bool>> where);
IQueryable<T>  GetAll();                         // repository.GetAll()
ApiResult<T>   Search(string restQuery);         // repository.GetAll(restQuery)

// IDataServiceWithXxxId<DBC,T>
Task<T?>       GetById(TId id);
Task<bool>     Delete(TId id, bool commit = true);
```

The base class keeps the repository in the protected field `repository` (typed as `IRepository<DBC,T>`).

### Custom data service

Keep a typed reference to your own repository interface for custom methods:

```csharp
public interface IProductService : IDataServiceWithIntId<AppDbContext, Product>
{
    Task<bool> AdjustStock(int productId, int quantity);
}

public class ProductService : BaseDataServiceWithIntId<AppDbContext, Product>, IProductService
{
    private readonly IProductRepository products;

    public ProductService(
        IProductRepository repository,
        ILogger<IDataServiceWithIntId<AppDbContext, Product>> logger)
        : base(repository, logger)
    {
        products = repository;
    }

    public async Task<bool> AdjustStock(int productId, int quantity)
    {
        var product = await products.GetById(productId);
        if (product is null) return false;

        if (product.Stock + quantity < 0)
            throw new InvalidOperationException("Insufficient stock");

        product.Stock += quantity;
        await products.Update(product);
        return true;
    }
}
```

---

## Unit of Work

`IUnitOfWork` is a contract only:

```csharp
public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
```

**Important:** every repository creates its **own** `DbContext` from `IDbContextFactory<DBC>` in its constructor. Changes staged with `commit: false` live only in that repository's context, so:

- Calling `SaveChangesAsync` on some other injected `DbContext` will **not** save them.
- Two repositories cannot share a single `SaveChanges` call or a transaction.
- An entity loaded through one repository is not tracked by another.

A unit of work over these repositories must therefore commit each repository:

```csharp
public class OrderUnitOfWork : IUnitOfWork
{
    public OrderUnitOfWork(IProductRepository products, IOrderRepository orders)
    {
        Products = products;
        Orders = orders;
    }

    public IProductRepository Products { get; }
    public IOrderRepository Orders { get; }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await Orders.Commit();
        await Products.Commit();   // not atomic with the line above
    }
}
```

If you need atomic multi-entity writes, put them in a single repository (using `DataContext`/`dbset` and related `DbSet`s on the same context) or use EF Core directly.

---

## Data Models

### `BaseModel`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IsDeleted` | `bool` | `false` | Soft-delete flag (not enforced by the base classes) |
| `Created` | `DateTime` | `DateTime.UtcNow` | Creation timestamp (re-set by `Add`) |
| `CreatedBy` | `string?` | `null` | Creator — populate yourself |
| `LastUpdated` | `DateTime?` | `null` | Set by `Add`; not set by `Update` |
| `LastUpdatedBy` | `string?` | `null` | Last updater — populate yourself |

All of these are marked `[Editable(false)]`.

### ID models

```csharp
public abstract class BaseModelWithIntId : BaseModel, IModelWithIntId
{
    public int Id { get; set; }
}

public abstract class BaseModelWithGuidId : BaseModel, IModelWithGuidId
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
}

public abstract class BaseModelWithStringId : BaseModel, IModelWithStringId
{
    [Required]
    public string Id { get; set; } = string.Empty;
}
```

### Lookup models

```csharp
public abstract class BaseLookupModel : BaseModelWithIntId, ILookupModel
{
    [Required]
    public required string Name { get; set; }
}

public class Category : BaseLookupModel
{
    public List<Product> Products { get; set; } = [];
}

var category = new Category { Name = "Electronics" };   // Name is a required member
```

### Users

```csharp
public abstract class BaseUser : IdentityUser, IBaseUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}
```

`IBaseTokenService<DBC, T>` (where `T : IdentityUser, IBaseUser`) defines `BuildAccessToken`, `ValidateToken`, `GenerateRefreshToken` and `RefreshAccessToken`; no implementation is provided.

### DTOs

```csharp
public record ApiResult<T>
{
    public IEnumerable<T> Data { get; init; } = [];
    public Pagination? Pagination { get; init; }      // null unless a page size was requested
}

public record Pagination
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int PageCount { get; init; }
    public int TotalCount { get; init; }
}

public record ProgressReport
{
    public int CurrentProgress { get; init; }
    public int TotalProgress { get; init; }
    public string Message { get; init; } = "Processing";
}

public record Credentials
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(256, MinimumLength = 12), DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;
}

public record RefreshTokenCredentials
{
    [Required] public string UserName { get; init; } = string.Empty;
    [Required] public string RefreshToken { get; init; } = string.Empty;
}

public class JWTSettings
{
    [Required, MinLength(32)] public required string SecretKey { get; set; }   // ≥ 256 bits for HMAC-SHA256
    [Required] public required string Issuer { get; set; }
    [Required] public required string Audience { get; set; }
    public int ExpiryMinutes { get; set; }
    public int RefreshTokenExpiryMinutes { get; set; }
}
```

### Validation helper

`Core.Common.Helpers.ObjectExtensions.IsValid` runs data-annotation validation (including all properties) on any object:

```csharp
using Core.Common.Helpers;

if (!settings.IsValid(out var errors))
    throw new InvalidOperationException(string.Join("; ", errors.Select(e => e.ErrorMessage)));
```

---

## REST Query Syntax

`Repository.GetAll(string)` and `DataService.Search(string)` pass the query to REST-Parser, which applies it to the `IQueryable<T>` and executes it.

### Basic format

```
field[operator]=value&field2=value2&$sort_by=field[ASC]&$page=1&$pagesize=20
```

### Examples

```http
# Filter by category and price
category=Electronics&price[lt]=1000

# Sort descending with pagination
$sort_by=price[DESC]&$page=1&$pagesize=20

# Contains search
name[contains]=Pro&$sort_by=price[DESC]

# Date range
releaseDate[ge]=2023-01-01&releaseDate[le]=2023-12-31

# Multiple sorts
category=Electronics&$sort_by=brand[ASC]&$sort_by=price[ASC]&$page=1&$pagesize=10
```

### Filtering operators

| Operator | Description | Supported types |
|----------|-------------|-----------------|
| `eq` | Equal to *(default)* | All types |
| `ne` | Not equal to | All types |
| `gt` | Greater than | int, double, decimal, DateTime |
| `ge` | Greater than or equal | int, double, decimal, DateTime |
| `lt` | Less than | int, double, decimal, DateTime |
| `le` | Less than or equal | int, double, decimal, DateTime |
| `contains` | Contains substring *(case-sensitive)* | string |

Supported field types: `string`, `int`, `double`, `decimal`, `DateTime`, `bool`, `Guid` and their nullable forms.

### Pagination and limits

| Limit | Value |
|-------|-------|
| Default page size (when `$pagesize` has no value) | 25 |
| Max page size | 1000 |
| Max filter conditions | 50 |
| Max query length | 2000 chars |

`ApiResult<T>.Pagination` is only populated when the result has a page size, i.e. when `$pagesize` was supplied. Serialised, a paged result looks like:

```json
{
  "data": [ ... ],
  "pagination": { "pageNumber": 1, "pageSize": 20, "pageCount": 5, "totalCount": 98 }
}
```

### Exception handling

```csharp
try
{
    return Ok(_productService.Search(q));
}
catch (REST_InvalidFieldnameException ex)
{
    return BadRequest(new { error = "Invalid field", message = ex.Message });
}
catch (REST_InvalidOperatorException ex)
{
    return BadRequest(new { error = "Invalid operator", message = ex.Message });
}
catch (REST_InvalidValueException ex)
{
    return BadRequest(new { error = "Invalid value", message = ex.Message });
}
catch (ArgumentException ex)
{
    // Query too long, too many conditions, or bad format
    return BadRequest(new { error = "Invalid query", message = ex.Message });
}
```

See [Docs/Rest-Parser-Usage.md](Docs/Rest-Parser-Usage.md) for the full syntax.

---

## Auditing

### Audit fields

The base repository sets `Created` and `LastUpdated` on `Add` only. `Update` leaves all audit fields as supplied by the caller, and because it marks every property modified, a client-supplied `Created`/`CreatedBy` will overwrite the stored values. Populate the user fields (and protect the creation fields) by overriding `Add`/`Update`:

```csharp
public class ProductRepository : BaseRepositoryWithIntId<AppDbContext, Product>, IProductRepository
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductRepository(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IRestToLinqParser<Product> parser,
        ILogger<IRepository<AppDbContext, Product>> logger,
        IHttpContextAccessor httpContextAccessor)
        : base(dbContextFactory, parser, logger)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private string CurrentUser =>
        _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";

    public override Task<Product> Add(Product entity, bool commit = true)
    {
        entity.CreatedBy = CurrentUser;
        entity.LastUpdatedBy = CurrentUser;
        return base.Add(entity, commit);
    }

    public override async Task<Product> Update(Product entity, bool commit = true)
    {
        entity.LastUpdated = DateTime.UtcNow;
        entity.LastUpdatedBy = CurrentUser;

        await base.Update(entity, commit: false);
        var entry = DataContext.Entry(entity);
        entry.Property(e => e.Created).IsModified = false;
        entry.Property(e => e.CreatedBy).IsModified = false;

        if (commit) await Commit();
        return entity;
    }
}
```

### `IAuditor` / `BaseAuditor`

`IAuditor` is a separate audit-log abstraction; the base repositories and services do **not** call it. `BaseAuditor` is an abstract class whose default `AuditAsync` writes `AUDIT: {Message}` to `ILogger`. Derive from it to persist elsewhere and call it from your own code:

```csharp
public class DatabaseAuditor : BaseAuditor
{
    private readonly IAuditLogRepository _auditRepo;

    public DatabaseAuditor(ILogger<BaseAuditor> logger, IAuditLogRepository auditRepo)
        : base(logger)
    {
        _auditRepo = auditRepo;
    }

    public override async Task AuditAsync(string message, CancellationToken cancellationToken = default)
        => await _auditRepo.Add(new AuditLog { Message = message, Timestamp = DateTime.UtcNow });
}

builder.Services.AddScoped<IAuditor, DatabaseAuditor>();
```

### Soft delete

All `Delete` methods are **hard** deletes, and `GetAll` / `GetById` / `Search` do not filter on `IsDeleted`. To use soft delete, set the flag and update, and filter explicitly — or add an EF Core global query filter:

```csharp
// Soft delete
product.IsDeleted = true;
await repository.Update(product);

// AppDbContext.OnModelCreating
modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
```

---

## Best Practices

1. **Register with `AddDbContextFactory`.** Repositories require `IDbContextFactory<DBC>`; `AddDbContext` alone will fail to resolve.
2. **Register repositories and services as scoped.** Each repository holds a `DbContext` for its lifetime; a singleton repository would share one context across requests.
3. **Never block on async** — `await repository.GetById(id)`, not `.Result`.
4. **Provide REST query defaults** (e.g. `$sort_by=Id&$page=1&$pagesize=20`) so unbounded queries are not run by accident.
5. **Use DTOs for API input/output.** Binding entities directly lets clients set `IsDeleted`, `Created`, `CreatedBy` etc. on `Update`.
6. **Use structured logging** in overrides — `logger.LogInformation("Adding product {Name}", model.Name)` — and avoid logging whole entities.

---

## Known Limitations

- **`AlwaysIncludeChildren`**: include paths are discovered by reflection as every property whose type is generic. This also picks up `Nullable<T>` properties — including `BaseModel.LastUpdated` — so enabling it causes EF Core to throw when the query runs. It also misses non-collection reference navigations, and it never applies to `GetById`. Prefer overriding `GetAll()` with explicit `.Include(...)` calls.
- **One `DbContext` per repository** — see [Unit of Work](#unit-of-work). The context is not disposed by the repository.
- **`AddBatch` batch sizes** are approximate: the first commit happens after `batchSize + 2` entities and subsequent ones every `batchSize + 1`.
- **`Delete(where)` always returns `true`**, so it cannot be used to detect "not found".

---

## Troubleshooting

| Issue | Cause | Fix |
|-------|-------|-----|
| `Unable to resolve service for type IDbContextFactory<...>` | Context registered with `AddDbContext` | Use `AddDbContextFactory<T>` |
| `Unable to resolve service for type IRestToLinqParser<T>` | Parser not registered | `builder.Services.RegisterRestParser<T>()` |
| `Unable to resolve service for type IRepositoryWithIntId<...>` | Service constructor asks for the generic interface but only `IProductRepository` is registered | Take `IProductRepository` in the service constructor, or register the generic interface too |
| Changes made with `commit: false` never saved | `SaveChanges` called on a different context | Call `Commit()` on the same repository |
| Navigation properties are null | Includes not applied | Override `GetAll()` with explicit `Include`s (see Known Limitations) |
| `CreatedBy` / `LastUpdatedBy` always null | Not populated by the base classes | Override `Add` / `Update` (see [Auditing](#auditing)) |
| `LastUpdated` not changing on update | `Update` doesn't set it | Override `Update` |
| `Pagination` is null | No `$pagesize` in the query | Include `$page=1&$pagesize=20` |
| `ArgumentException` on query | Query too long or too many conditions | Max 2000 chars, max 50 conditions |

---

## Additional Resources

- [Docs/SilverCodeAPI-Usage-Guide.md](Docs/SilverCodeAPI-Usage-Guide.md) — extended usage guide
- [Docs/Rest-Parser-Usage.md](Docs/Rest-Parser-Usage.md) — REST-Parser query syntax
- [REST-Parser on GitHub](https://github.com/BigBadJock/REST-Parser) · [REST-Parser on NuGet](https://www.nuget.org/packages/REST-Parser)
- [SilverCodeAPI GitHub Packages](https://github.com/BigBadJock/SilverCodeAPI/packages)

## Contributing

Pull requests are welcome. Please open an issue first to discuss significant changes.

---

**Target Framework**: .NET 10 | **Author**: John McArthur
