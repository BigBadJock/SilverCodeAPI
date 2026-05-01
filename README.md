# SilverCodeAPI

[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/badge/NuGet-available-blue)](https://github.com/BigBadJock/SilverCodeAPI/packages)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A .NET 10 library providing infrastructure for data access patterns including **Repository**, **Unit of Work**, and **Data Service** implementations with built-in REST query support via [REST-Parser](https://github.com/BigBadJock/REST-Parser).

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
- [Repository Pattern](#repository-pattern)
- [Data Services](#data-services)
- [Unit of Work](#unit-of-work)
- [Data Models](#data-models)
- [REST Query Integration](#rest-query-integration)
- [Auditing](#auditing)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---


## Overview

SilverCodeAPI is a collection of NuGet packages that provide:

- **Generic Repository Pattern** — CRUD operations with type-safe querying
- **Data Service Layer** — Business logic abstraction over repositories
- **REST Query Support** — Integrated with REST-Parser for URL-driven filtering, sorting and pagination
- **Multiple ID Types** — Support for `int`, `Guid`, and `string` identifiers
- **Audit Tracking** — Built-in creation and modification tracking
- **Unit of Work** — Transaction management across repositories
- **Read-Only Repositories** — Separate interfaces for read and write operations

### Packages

| Package | Description |
|---------|-------------|
| `Core.Common.Contracts` | Interfaces for repositories, data services, and contracts |
| `Core.Common.DataModels` | Base entity models and DTOs |
| `Core.Common` | Concrete implementations of repository and service patterns |

---

## Architecture

```
┌─────────────────────────────────────────────────────┐
│              Your API Controller                     │
└──────────────────┬──────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────┐
│           Data Service Layer                         │
│  (BaseDataService, BaseDataServiceWithIntId, etc.)   │
└──────────────────┬──────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────┐
│           Repository Layer                           │
│  (BaseRepository, BaseRepositoryWithIntId, etc.)     │
└──────────────────┬──────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────┐
│         Entity Framework Core DbContext              │
└─────────────────────────────────────────────────────┘
```

---

## Installation

### Package Manager Console

```powershell
Install-Package Core.Common.Contracts
Install-Package Core.Common.DataModels
Install-Package Core.Common
Install-Package REST-Parser
```

### .NET CLI

```bash
dotnet add package Core.Common.Contracts
dotnet add package Core.Common.DataModels
dotnet add package Core.Common
dotnet add package REST-Parser
```

### Package References

```xml
<ItemGroup>
  <PackageReference Include="Core.Common.Contracts" Version="1.2026.*" />
  <PackageReference Include="Core.Common.DataModels" Version="1.2026.*" />
  <PackageReference Include="Core.Common" Version="1.2026.*" />
  <PackageReference Include="REST-Parser" Version="1.2.5" />
</ItemGroup>
```

### GitHub Packages

1. Get a personal access token from **GitHub → Settings → Developer Settings → Personal Access Tokens**
2. Run: `nuget setApiKey <accesstoken> -source github`
3. Add a `nuget.config` to your project root (add it to `.gitignore` — it contains your token):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json"/>
    <add key="github" value="https://nuget.pkg.github.com/bigbadjock/index.json"/>
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="UserName" value="bigbadjock"/>
      <add key="ClearTextPassword" value="<accessToken>"/>
    </github>
  </packageSourceCredentials>
</configuration>
```

---

## Quick Start

### 1. Define Your Entity

Choose a base model based on your ID type:

```csharp
using Core.Common.DataModels;

// Integer ID
public class Product : BaseModelWithIntId
{
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

// GUID ID
public class Order : BaseModelWithGuidId
{
    public string OrderNumber { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
}

// String ID
public class UserProfile : BaseModelWithStringId
{
    public string Username { get; set; }
    public string Email { get; set; }
}
```

### 2. Create Your DbContext

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
}
```

### 3. Implement a Repository

```csharp
using Core.Common;
using Core.Common.Contracts;

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

### 4. Implement a Data Service

```csharp
public interface IProductService : IDataServiceWithIntId<AppDbContext, Product> { }

public class ProductService : BaseDataServiceWithIntId<AppDbContext, Product>, IProductService
{
    public ProductService(
        IRepositoryWithIntId<AppDbContext, Product> repository,
        ILogger<IDataServiceWithIntId<AppDbContext, Product>> logger)
        : base(repository, logger) { }
}
```

### 5. Register Services

```csharp
// Program.cs
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.RegisterRestParser<Product>();
builder.Services.RegisterRestParser<Order>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
```

### 6. Use in a Controller

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

    // GET: api/products?category=Electronics&price[lt]=1000&$sort_by=price[ASC]&$page=1&$pagesize=20
    [HttpGet]
    public IActionResult Search([FromQuery] string q = "$sort_by=Id&$pagesize=20")
    {
        var result = _productService.Search(q);
        return Ok(new { data = result.Data, pagination = result.Pagination });
    }

    [HttpGet("{id}")]
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        if (id != product.Id) return BadRequest();
        return Ok(await _productService.Update(product));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _productService.Delete(p => p.Id == id);
        return success ? NoContent() : NotFound();
    }
}
```

---

## Core Concepts

### ID Type Variants

SilverCodeAPI provides three sets of base classes for different ID types:

| Variant | Model | Repository | Service |
|---------|-------|------------|---------|
| Integer | `BaseModelWithIntId` | `BaseRepositoryWithIntId<DBC,T>` | `BaseDataServiceWithIntId<DBC,T>` |
| GUID | `BaseModelWithGuidId` | `BaseRepositoryWithGuidId<DBC,T>` | `BaseDataServiceWithGuidId<DBC,T>` |
| String | `BaseModelWithStringId` | `BaseRepositoryWithStringId<DBC,T>` | `BaseDataServiceWithStringId<DBC,T>` |

Read-only variants are also available:

| Variant | Class |
|---------|-------|
| Integer | `BaseReadRepositoryWithIntId<DBC,T>` |
| GUID | `BaseReadRepositoryWithGuidId<DBC,T>` |
| String | `BaseReadRepositoryWithStringId<DBC,T>` |

---

## Repository Pattern

### Available Methods

```csharp
// Get all records as IQueryable
IQueryable<Product> products = repository.GetAll();

// Get with REST query — returns ApiResult<T> with pagination metadata
ApiResult<Product> result = repository.GetAll("category=Electronics&price[lt]=1000");

// Get by ID — returns null if not found
Product? product = await repository.GetById(5);

// Add entity (commits immediately by default)
Product newProduct = await repository.Add(product);

// Add without immediate commit
await repository.Add(product, commit: false);
await repository.Commit();

// Batch add with progress reporting
var progress = new Progress<ProgressReport>(r =>
    Console.WriteLine($"{r.Message}: {r.CurrentProgress}/{r.TotalProgress}"));

await repository.AddBatch(products, batchSize: 100, progress);

// Update
Product updated = await repository.Update(product);

// Delete by predicate
bool deleted = await repository.Delete(p => p.Id == 5);

// Delete by ID (typed repositories)
bool deleted = await repository.Delete(5);
```

### Custom Repository Methods

```csharp
public interface IProductRepository : IRepositoryWithIntId<AppDbContext, Product>
{
    Task<IEnumerable<Product>> GetLowStockProducts(int threshold);
    Task<decimal> GetAveragePriceByCategory(string category);
}

public class ProductRepository : BaseRepositoryWithIntId<AppDbContext, Product>, IProductRepository
{
    public ProductRepository(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IRestToLinqParser<Product> parser,
        ILogger<IRepository<AppDbContext, Product>> logger)
        : base(dbContextFactory, parser, logger) { }

    public async Task<IEnumerable<Product>> GetLowStockProducts(int threshold)
        => await dbset.Where(p => p.Stock < threshold && !p.IsDeleted).ToListAsync();

    public async Task<decimal> GetAveragePriceByCategory(string category)
        => await dbset.Where(p => p.Category == category && !p.IsDeleted)
                      .AverageAsync(p => p.Price);
}
```

---

## Data Services

### Built-in Methods

```csharp
// All data services provide:
Task<T>           Add(T model)
Task<T>           Update(T model)
Task<bool>        Delete(Expression<Func<T, bool>> where)
ApiResult<T>      Search(string restQuery)

// ID-typed services additionally provide:
Task<T?>          GetById(int id)      // int variant
Task<T?>          GetById(Guid id)     // Guid variant
Task<T?>          GetById(string id)   // string variant
Task<bool>        Delete(int id, ...)  // etc.
```

### Custom Data Service

```csharp
public interface IProductService : IDataServiceWithIntId<AppDbContext, Product>
{
    Task<bool> AdjustStock(int productId, int quantity);
    Task<IEnumerable<Product>> GetProductsNeedingRestock(int threshold);
}

public class ProductService : BaseDataServiceWithIntId<AppDbContext, Product>, IProductService
{
    private new readonly IProductRepository repository;

    public ProductService(
        IRepositoryWithIntId<AppDbContext, Product> repository,
        ILogger<IDataServiceWithIntId<AppDbContext, Product>> logger)
        : base(repository, logger)
    {
        this.repository = (IProductRepository)repository;
    }

    public async Task<bool> AdjustStock(int productId, int quantity)
    {
        var product = await repository.GetById(productId);
        if (product is null) return false;

        product.Stock += quantity;
        if (product.Stock < 0)
            throw new InvalidOperationException("Insufficient stock");

        await repository.Update(product);
        return true;
    }

    public async Task<IEnumerable<Product>> GetProductsNeedingRestock(int threshold)
        => await repository.GetLowStockProducts(threshold);
}
```

---

## Unit of Work

Use `IUnitOfWork` to coordinate multiple repositories in a single transaction:

```csharp
public interface IAppUnitOfWork : IUnitOfWork
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
}

public class AppUnitOfWork : IAppUnitOfWork
{
    private readonly AppDbContext _context;

    public AppUnitOfWork(
        AppDbContext context,
        IProductRepository productRepository,
        IOrderRepository orderRepository)
    {
        _context = context;
        Products = productRepository;
        Orders = orderRepository;
    }

    public IProductRepository Products { get; }
    public IOrderRepository Orders { get; }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}

// Usage
public async Task<Order> CreateOrderWithStockUpdate(Order order, int productId, int quantity)
{
    var createdOrder = await _unitOfWork.Orders.Add(order, commit: false);

    var product = await _unitOfWork.Products.GetById(productId);
    if (product is null) throw new InvalidOperationException("Product not found");
    product.Stock -= quantity;
    await _unitOfWork.Products.Update(product, commit: false);

    await _unitOfWork.CommitAsync();
    return createdOrder;
}
```

---

## Data Models

### Base Model Properties

All entities inherit these audit fields from `BaseModel`:

| Property | Type | Description |
|----------|------|-------------|
| `IsDeleted` | `bool` | Soft-delete flag (default `false`) |
| `Created` | `DateTime` | UTC creation timestamp |
| `CreatedBy` | `string?` | Identity of creator |
| `LastUpdated` | `DateTime?` | UTC last-update timestamp |
| `LastUpdatedBy` | `string?` | Identity of last updater |

### ID Models

```csharp
public abstract class BaseModelWithIntId : BaseModel
{
    public int Id { get; set; }
}

public abstract class BaseModelWithGuidId : BaseModel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
}

public abstract class BaseModelWithStringId : BaseModel
{
    [Required]
    public string Id { get; set; } = string.Empty;
}
```

### Lookup Models

For reference/lookup data with integer IDs:

```csharp
public abstract class BaseLookupModel : BaseModelWithIntId, ILookupModel
{
    [Required]
    public string Name { get; set; }
}

// Usage
public class Category : BaseLookupModel
{
    // Inherits: Id, Name, IsDeleted, Created, CreatedBy, etc.
    public List<Product> Products { get; set; }
}
```

### DTOs

The following result types are `record`s with `init`-only properties:

```csharp
public record ApiResult<T>
{
    public IEnumerable<T> Data { get; init; } = [];
    public Pagination? Pagination { get; init; }
}

public record Pagination
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int PageCount { get; init; }
    public int TotalCount { get; init; }
}

public record Credentials
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(256, MinimumLength = 12)]
    [DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;
}
```

---

## REST Query Integration

SilverCodeAPI integrates with [REST-Parser](https://github.com/BigBadJock/REST-Parser) to allow URL-driven querying of any entity.

### Query Examples

```http
# Filter by category and price
GET /api/products?category=Electronics&price[lt]=1000

# Sort descending with pagination
GET /api/products?$sort_by=price[DESC]&$page=1&$pagesize=20

# Contains search
GET /api/products?name[contains]=Pro&$sort_by=price[DESC]

# Date range
GET /api/products?releaseDate[ge]=2023-01-01&releaseDate[le]=2023-12-31

# Complex query
GET /api/products?category=Electronics&price[ge]=100&price[le]=500&stock[gt]=5&$sort_by=name[ASC]&$page=1&$pagesize=10
```

### Filtering Operators

| Operator | Description | Supported Types |
|----------|-------------|-----------------|
| `eq` | Equal to *(default)* | All types |
| `ne` | Not equal to | All types |
| `gt` | Greater than | int, double, decimal, DateTime |
| `ge` | Greater than or equal | int, double, decimal, DateTime |
| `lt` | Less than | int, double, decimal, DateTime |
| `le` | Less than or equal | int, double, decimal, DateTime |
| `contains` | Contains substring *(case-sensitive)* | string |

### Pagination Limits

| Limit | Value |
|-------|-------|
| Max query length | 2000 chars |
| Max filter conditions | 50 |
| Max page size | 1000 |

### Exception Handling

```csharp
try
{
    var result = _productService.Search(q ?? "$sort_by=Id&$pagesize=20");
    return Ok(result);
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

---

## Auditing

### Built-in Audit Fields

`Created` and `LastUpdated` are set automatically in UTC by the base repository on `Add` and `Update`. `CreatedBy` and `LastUpdatedBy` are available but require a custom auditor to populate them.

### Custom Auditor

Implement `BaseAuditor` to write to a dedicated audit store. The default implementation logs via `ILogger`:

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
    {
        await _auditRepo.Add(new AuditLog { Message = message, Timestamp = DateTime.UtcNow });
    }
}

// Register in DI
builder.Services.AddScoped<IAuditor, DatabaseAuditor>();
```

### Populating Audit Fields in a Repository

```csharp
public class ProductRepository : BaseRepositoryWithIntId<AppDbContext, Product>
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

    public override async Task<Product> Add(Product entity, bool commit = true)
    {
        entity.CreatedBy = CurrentUser;
        return await base.Add(entity, commit);
    }

    public override async Task<Product> Update(Product entity, bool commit = true)
    {
        entity.LastUpdatedBy = CurrentUser;
        return await base.Update(entity, commit);
    }
}
```

### Soft Delete

```csharp
public async Task<bool> SoftDelete(int productId)
{
    var product = await repository.GetById(productId);
    if (product is null) return false;

    product.IsDeleted = true;
    product.LastUpdated = DateTime.UtcNow;
    await repository.Update(product);
    return true;
}

// Filter soft-deleted records in queries
public IQueryable<Product> GetActiveProducts()
    => repository.GetAll().Where(p => !p.IsDeleted);
```

---

## Best Practices

### 1. Always Use `AddDbContextFactory`

```csharp
// ✅ Correct — required by the repository base classes
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ❌ Avoid — repositories use IDbContextFactory<T> directly
builder.Services.AddDbContext<AppDbContext>(...);
```

### 2. Never Block on Async

```csharp
// ✅ Correct
var product = await repository.GetById(id);

// ❌ Deadlock risk
var product = repository.GetById(id).Result;
```

### 3. Provide REST Query Defaults

```csharp
[HttpGet]
public IActionResult Search([FromQuery] string q = "")
{
    if (string.IsNullOrWhiteSpace(q))
        q = "$sort_by=Id&$pagesize=20";

    var result = _service.Search(q);
    return Ok(result);
}
```

### 4. Use DTOs for API Responses

Don't expose entity models directly — exclude audit and soft-delete fields from responses:

```csharp
var dto = new ProductDto
{
    Id = product.Id,
    Name = product.Name,
    Price = product.Price
    // Excludes: CreatedBy, LastUpdatedBy, IsDeleted
};
```

### 5. Use `AsNoTracking` for Read-Only Queries

```csharp
var result = _parser.Run(_context.Products.AsNoTracking(), query);
```

### 6. Structured Logging

```csharp
logger.LogInformation("Adding product {Name}", model.Name);
logger.LogError(ex, "Failed to update product {Id}", model.Id);
// Never log full entity objects — they may contain PII
```

---

## Migration Guide

### From Direct EF Core to SilverCodeAPI

**Before:**
```csharp
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    [HttpGet]
    public async Task<IActionResult> GetProducts()
        => Ok(await _context.Products.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return Ok(product);
    }
}
```

**After:**
```csharp
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    [HttpGet]
    public IActionResult GetProducts([FromQuery] string q = "")
        => Ok(_productService.Search(q ?? "$pagesize=20"));

    [HttpPost]
    public async Task<IActionResult> CreateProduct(Product product)
        => Ok(await _productService.Add(product));
}
```

### Migration Steps

1. Add NuGet packages
2. Update entity models to inherit from the appropriate `BaseModel*` class
3. Create repository interfaces and implementations
4. Create data service interfaces and implementations
5. Update DI registrations — swap `AddDbContext` for `AddDbContextFactory`
6. Register REST parsers: `builder.Services.RegisterRestParser<T>()`
7. Update controllers to inject services instead of `DbContext`

---

## Troubleshooting

| Issue | Cause | Fix |
|-------|-------|-----|
| `DbContext has been disposed` | Lifecycle mismatch | Use `AddDbContextFactory<T>` not `AddDbContext<T>` |
| REST parser not found in DI | Parser not registered | Add `builder.Services.RegisterRestParser<T>()` |
| Navigation properties are null | Includes not enabled | Set `repository.AlwaysIncludeChildren = true` or override `GetAll()` |
| Audit fields always null | No auditor injected | Implement `BaseAuditor` and register it as `IAuditor` |
| Pagination not returned | Missing `$page`/`$pagesize` | Include `$page=1&$pagesize=20` in the query string |
| `ArgumentException` on query | Query too long or too many conditions | Max 2000 chars, max 50 conditions |

---

## Additional Resources

- **REST-Parser Usage Guide**: [Docs/Rest-Parser-Usage.md](Docs/Rest-Parser-Usage.md)
- **GitHub Repository**: https://github.com/BigBadJock/SilverCodeAPI
- **NuGet Packages**:
  - https://www.nuget.org/packages/Core.Common.Contracts/
  - https://www.nuget.org/packages/Core.Common.DataModels/
  - https://www.nuget.org/packages/Core.Common/

---

**Target Framework**: .NET 10 | **Author**: John McArthur | **License**: MIT


A set of .NET 10 NuGet packages providing interfaces and abstract base classes for building API services using the **Repository Pattern** and **Unit of Work**. Includes built-in support for [REST-Parser](https://github.com/BigBadJock/REST-Parser), enabling fully featured URL-driven search, filtering, sorting, and pagination out of the box.

---

## Packages

| Package | Description |
|---------|-------------|
| `Core.Common` | Abstract base implementations for repositories and data services |
| `Core.Common.Contracts` | Interfaces and contracts |
| `Core.Common.DataModels` | Base data models and entity definitions |

---

## Installation

### NuGet Package Manager
```bash
Install-Package Core.Common
Install-Package Core.Common.Contracts
Install-Package Core.Common.DataModels
```

### .NET CLI
```bash
dotnet add package Core.Common
dotnet add package Core.Common.Contracts
dotnet add package Core.Common.DataModels
```

### GitHub Packages

1. Get a personal access token from **GitHub → Settings → Developer Settings → Personal Access Tokens**
2. Run: `nuget setApiKey <accesstoken> -source github`
3. Add a `nuget.config` file to your project root (add `nuget.config` to `.gitignore` — it contains your token):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json"/>
    <add key="github" value="https://nuget.pkg.github.com/bigbadjock/index.json"/>
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="UserName" value="bigbadjock"/>
      <add key="ClearTextPassword" value="<accessToken>"/>
    </github>
  </packageSourceCredentials>
</configuration>
```

---

## Quick Start

### 1. Define Your Entity

Inherit from one of the base model classes matching your ID type:

```csharp
// Integer ID
public class Product : BaseModelWithIntId
{
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}

// GUID ID
public class Order : BaseModelWithGuidId { ... }

// String ID
public class Tag : BaseModelWithStringId { ... }
```

All base models include built-in audit fields:

| Field | Type | Description |
|-------|------|-------------|
| `Id` | int / Guid / string | Primary key |
| `Created` | `DateTime` | UTC timestamp set on creation |
| `CreatedBy` | `string?` | User who created the record |
| `LastUpdated` | `DateTime?` | UTC timestamp of last update |
| `LastUpdatedBy` | `string?` | User who last updated the record |
| `IsDeleted` | `bool` | Soft-delete flag |

### 2. Create a Repository

```csharp
public class ProductRepository : BaseRepositoryWithIntId<AppDbContext, Product>, IProductRepository
{
    public ProductRepository(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IRestToLinqParser<Product> parser,
        ILogger<IRepository<AppDbContext, Product>> logger)
        : base(dbContextFactory, parser, logger)
    {
    }
}
```

### 3. Create a Data Service

```csharp
public class ProductService : BaseDataServiceWithIntId<AppDbContext, Product>, IProductService
{
    public ProductService(
        IRepositoryWithIntId<AppDbContext, Product> repository,
        ILogger<IDataServiceWithIntId<AppDbContext, Product>> logger)
        : base(repository, logger)
    {
    }
}
```

### 4. Register Dependencies

```csharp
// Program.cs
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.RegisterRestParser<Product>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
```

### 5. Use in a Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Get([FromQuery] string q = "$sort_by=Id&$page=1&$pagesize=20")
    {
        var result = _service.Search(q);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _service.GetById(id);
        return product is null ? NotFound() : Ok(product);
    }
}
```

---

## REST Query Syntax

The built-in REST-Parser lets API consumers filter, sort, and paginate results directly via URL query strings — no extra endpoint logic required.

### Basic Format

```
GET /api/products?field[operator]=value&field2=value2
```

### Quick Examples

```http
# Filter by category and price
GET /api/products?category=Electronics&price[lt]=1000

# Sort descending, paginate
GET /api/products?$sort_by=price[DESC]&$page=1&$pagesize=20

# Combined: filter + multi-sort + paginate
GET /api/products?category=Electronics&isActive=true&$sort_by=brand[ASC]&$sort_by=price[ASC]&$page=1&$pagesize=10

# Date range
GET /api/products?releaseDate[ge]=2023-01-01&releaseDate[le]=2023-12-31
```

### Filtering Operators

| Operator | Description | Supported Types |
|----------|-------------|-----------------|
| `eq` | Equal to *(default)* | All types |
| `ne` | Not equal to | All types |
| `gt` | Greater than | int, double, decimal, DateTime |
| `ge` | Greater than or equal | int, double, decimal, DateTime |
| `lt` | Less than | int, double, decimal, DateTime |
| `le` | Less than or equal | int, double, decimal, DateTime |
| `contains` | Contains substring *(case-sensitive)* | string |

### Supported Field Types

`string` · `int` / `int?` · `double` / `double?` · `decimal` / `decimal?` · `DateTime` / `DateTime?` · `bool` / `bool?` · `Guid` / `Guid?`

### Sorting

```
$sort_by=field[ASC]    # ascending
$sort_by=field[DESC]   # descending

# Multiple sorts
$sort_by=category[ASC]&$sort_by=price[DESC]
```

If no `$sort_by` is provided, results default to `Id ASC`.

### Pagination

```
$page=2&$pagesize=25
```

| Limit | Default | Maximum |
|-------|---------|---------|
| Page size | 25 | 1000 |
| Conditions | — | 50 |
| Query length | — | 2000 chars |

### Pagination Response (`ApiResult<T>`)

```json
{
  "data": [ ... ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 20,
    "pageCount": 5,
    "totalCount": 98
  }
}
```

---

## Exception Handling

The REST-Parser throws typed exceptions you can map to HTTP responses:

```csharp
try
{
    var result = _service.Search(q);
    return Ok(result);
}
catch (REST_InvalidFieldnameException ex)
{
    return BadRequest(new { error = "Invalid field name", message = ex.Message });
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
    // Query too long, too many conditions, or invalid format
    return BadRequest(new { error = "Invalid query", message = ex.Message });
}
```

---

## Architecture

```
Core.Common.Contracts          Core.Common                    Core.Common.DataModels
─────────────────────          ────────────────               ──────────────────────
IReadRepository<DBC,T>    ←─   BaseReadRepository             BaseModel
IRepository<DBC,T>        ←─   BaseRepository                 BaseModelWithIntId
IRepositoryWithIntId      ←─   BaseRepositoryWithIntId        BaseModelWithGuidId
IRepositoryWithGuidId     ←─   BaseRepositoryWithGuidId       BaseModelWithStringId
IRepositoryWithStringId   ←─   BaseRepositoryWithStringId     BaseLookupModel

IDataService<DBC,T>       ←─   BaseDataService
IDataServiceWithIntId     ←─   BaseDataServiceWithIntId       Credentials
IDataServiceWithGuidId    ←─   BaseDataServiceWithGuidId      RefreshTokenCredentials
IDataServiceWithStringId  ←─   BaseDataServiceWithStringId    JWTSettings
                                                               ApiResult<T>
IUnitOfWork                                                    Pagination
IAuditor              ←─       BaseAuditor
IBaseTokenService
```

---

## Available Base Classes

### Repositories

| Class | ID Type | Use When |
|-------|---------|----------|
| `BaseRepositoryWithIntId<DBC,T>` | `int` | Standard auto-increment PK |
| `BaseRepositoryWithGuidId<DBC,T>` | `Guid` | Distributed / globally unique PK |
| `BaseRepositoryWithStringId<DBC,T>` | `string` | Natural or user-defined PK |
| `BaseReadRepositoryWithIntId<DBC,T>` | `int` | Read-only repository |
| `BaseReadRepositoryWithGuidId<DBC,T>` | `Guid` | Read-only repository |
| `BaseReadRepositoryWithStringId<DBC,T>` | `string` | Read-only repository |

All repositories expose:
- `GetAll()` — returns `IQueryable<T>`
- `GetAll(string restQuery)` — returns filtered/sorted/paged `ApiResult<T>`
- `GetById(id)` — returns `T?`
- `Add(T entity)` · `Update(T entity)` · `Delete(...)` · `AddBatch(...)` · `Commit()`

### Data Services

| Class | ID Type |
|-------|---------|
| `BaseDataServiceWithIntId<DBC,T>` | `int` |
| `BaseDataServiceWithGuidId<DBC,T>` | `Guid` |
| `BaseDataServiceWithStringId<DBC,T>` | `string` |

---

## Security & Limits

- Query strings are validated against length (2000 chars) and condition count (50) limits — `ArgumentException` is thrown if exceeded
- `JWTSettings.SecretKey` is enforced to a minimum of 32 characters (256-bit) at the model validation level
- Entity data is never serialised into log output to prevent PII leakage
- All timestamps are stored in UTC

---

## Contributing

Pull requests are welcome. Please open an issue first to discuss significant changes.

---

## Links

- [REST-Parser GitHub](https://github.com/BigBadJock/REST-Parser)
- [REST-Parser NuGet](https://www.nuget.org/packages/REST-Parser)
- [SilverCodeAPI GitHub Packages](https://github.com/BigBadJock/SilverCodeAPI/packages)
