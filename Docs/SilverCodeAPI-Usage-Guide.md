# SilverCodeAPI Usage Guide

A .NET 10 library providing infrastructure for data access patterns including Repository, Unit of Work, and Data Service implementations with built-in REST query support.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
- [Repository Pattern](#repository-pattern)
- [Data Services](#data-services)
- [Database Factory & Unit of Work](#database-factory--unit-of-work)
- [Data Models](#data-models)
- [REST Query Integration](#rest-query-integration)
- [Auditing](#auditing)
- [Complete Examples](#complete-examples)
- [Best Practices](#best-practices)
- [Migration Guide](#migration-guide)
- [Troubleshooting](#troubleshooting)

---

## Overview

SilverCodeAPI is a collection of NuGet packages that provide:

- **Generic Repository Pattern** - CRUD operations with type-safe querying
- **Data Service Layer** - Business logic abstraction over repositories
- **REST Query Support** - Integrated with REST-Parser for flexible querying
- **Multiple ID Types** - Support for `int`, `Guid`, and `string` identifiers
- **Audit Tracking** - Built-in creation and modification tracking
- **Unit of Work** - Transaction management across repositories
- **Read-Only Repositories** - Separate interfaces for read and write operations

### NuGet Packages

| Package | Description |
|---------|-------------|
| `Core.Common.Contracts` | Interfaces for repositories, data services, and factories |
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
  <PackageReference Include="Core.Common.Contracts" Version="1.2025.*" />
  <PackageReference Include="Core.Common.DataModels" Version="1.2025.*" />
  <PackageReference Include="Core.Common" Version="1.2025.*" />
  <PackageReference Include="REST-Parser" Version="1.2.5" />
</ItemGroup>
```

---

## Quick Start

### 1. Define Your Entity

Choose a base model based on your ID type:

```csharp
using Core.Common.DataModels;

// Using integer ID
public class Product : BaseModelWithIntId
{
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

// Using GUID ID
public class Order : BaseModelWithGuidId
{
    public string OrderNumber { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
}

// Using string ID
public class UserProfile : BaseModelWithStringId
{
    public string Username { get; set; }
    public string Email { get; set; }
}
```

### 2. Create Your DbContext

```csharp
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
}
```

### 3. Implement Repository

```csharp
using Core.Common;
using Core.Common.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using REST_Parser;

public interface IProductRepository : IRepositoryWithIntId<AppDbContext, Product>
{
    // Add custom methods if needed
}

public class ProductRepository : BaseRepositoryWithIntId<AppDbContext, Product>, IProductRepository
{
    public ProductRepository(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IRestToLinqParser<Product> parser,
        ILogger<IRepositoryWithIntId<AppDbContext, Product>> logger)
        : base(dbContextFactory, parser, logger)
    {
    }

    // Implement custom methods here
}
```

### 4. Implement Data Service

```csharp
using Core.Common;
using Core.Common.Contracts;
using Microsoft.Extensions.Logging;

public interface IProductService : IDataServiceWithIntId<AppDbContext, Product>
{
    // Add custom business logic methods
    Task<Product> GetProductByName(string name);
}

public class ProductService : BaseDataServiceWithIntId<AppDbContext, Product>, IProductService
{
    public ProductService(
        IRepositoryWithIntId<AppDbContext, Product> repository,
        ILogger<IDataServiceWithIntId<AppDbContext, Product>> logger)
        : base(repository, logger)
    {
    }

    public async Task<Product> GetProductByName(string name)
    {
        return await repository.GetById(p => p.Name == name);
    }
}
```

### 5. Register Services

```csharp
using Microsoft.EntityFrameworkCore;
using REST_Parser.DependencyResolution;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// REST Parser
builder.Services.RegisterRestParser<Product>();
builder.Services.RegisterRestParser<Order>();
builder.Services.RegisterRestParser<UserProfile>();

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Data Services
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();
```

### 6. Use in Controller

```csharp
using Core.Common.DataModels;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult Search([FromQuery] string q = "")
    {
        var result = _productService.Search(q ?? "$sort_by=Id&$pagesize=20");
        
        return Ok(new
        {
            data = result.Data,
            pagination = result.Pagination
        });
    }

    // GET: api/products/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var product = await _productService.GetById(id);
        return product != null ? Ok(product) : NotFound();
    }

    // POST: api/products
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        var created = await _productService.Add(product);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    // PUT: api/products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        if (id != product.Id)
            return BadRequest();

        var updated = await _productService.Update(product);
        return Ok(updated);
    }

    // DELETE: api/products/5
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

#### Integer IDs

```csharp
// Base Model
public class Product : BaseModelWithIntId { }

// Repository Interface
public interface IProductRepository : IRepositoryWithIntId<AppDbContext, Product> { }

// Repository Implementation
public class ProductRepository : BaseRepositoryWithIntId<AppDbContext, Product> { }

// Data Service Interface
public interface IProductService : IDataServiceWithIntId<AppDbContext, Product> { }

// Data Service Implementation
public class ProductService : BaseDataServiceWithIntId<AppDbContext, Product> { }
```

#### GUID IDs

```csharp
// Base Model
public class Order : BaseModelWithGuidId { }

// Repository Interface
public interface IOrderRepository : IRepositoryWithGuidId<AppDbContext, Order> { }

// Repository Implementation
public class OrderRepository : BaseRepositoryWithGuidId<AppDbContext, Order> { }

// Data Service Interface
public interface IOrderService : IDataServiceWithGuidId<AppDbContext, Order> { }

// Data Service Implementation
public class OrderService : BaseDataServiceWithGuidId<AppDbContext, Order> { }
```

#### String IDs

```csharp
// Base Model
public class UserProfile : BaseModelWithStringId { }

// Repository Interface
public interface IUserProfileRepository : IRepositoryWithStringId<AppDbContext, UserProfile> { }

// Repository Implementation
public class UserProfileRepository : BaseRepositoryWithStringId<AppDbContext, UserProfile> { }

// Data Service Interface
public interface IUserProfileService : IDataServiceWithStringId<AppDbContext, UserProfile> { }

// Data Service Implementation
public class UserProfileService : BaseDataServiceWithStringId<AppDbContext, UserProfile> { }
```

### Read-Only Operations

Separate read-only interfaces for query-only scenarios:

```csharp
// Read-only repository
public interface IProductReadOnlyRepository : IReadRepositoryWithIntId<AppDbContext, Product> { }

public class ProductReadOnlyRepository : BaseReadRepositoryWithIntId<AppDbContext, Product>, IProductReadOnlyRepository
{
    public ProductReadOnlyRepository(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IRestToLinqParser<Product> parser,
        ILogger<IReadRepositoryWithIntId<AppDbContext, Product>> logger)
        : base(dbContextFactory, parser, logger)
    {
    }
}
```

---

## Repository Pattern

### Available Methods

#### Read Operations

```csharp
// Get all records
IQueryable<Product> products = repository.GetAll();

// Get with REST query
ApiResult<Product> result = repository.GetAll("category=Electronics&price[lt]=1000");

// Get by ID
Product product = await repository.GetById(5);

// Get by ID with child entities
Product productWithChildren = await repository.GetById(5, includeChildren: true);

// Get by condition
Product product = await repository.GetById(p => p.Name == "iPhone");

// Get multiple by condition
IEnumerable<Product> products = repository.GetMany(p => p.Category == "Electronics");
```

#### Write Operations

```csharp
// Add entity
Product newProduct = await repository.Add(product);

// Add without immediate commit
Product newProduct = await repository.Add(product, commit: false);
await repository.Commit(); // Commit later

// Batch add with progress reporting
IProgress<ProgressReport> progress = new Progress<ProgressReport>(report =>
{
    Console.WriteLine($"{report.Message}: {report.CurrentProgress}/{report.TotalProgress}");
});

await repository.AddBatch(products, batchSize: 100, progress);

// Update entity
Product updated = await repository.Update(product);

// Delete by condition
bool deleted = await repository.Delete(p => p.Id == 5);

// Delete entity
bool deleted = await repository.Delete(product);

// Manual commit
await repository.Commit();
```

### Custom Repository Methods

```csharp
public interface IProductRepository : IRepositoryWithIntId<AppDbContext, Product>
{
    Task<IEnumerable<Product>> GetLowStockProducts(int threshold);
    Task<IEnumerable<Product>> GetProductsByCategory(string category);
    Task<decimal> GetAveragePriceByCategory(string category);
}

public class ProductRepository : BaseRepositoryWithIntId<AppDbContext, Product>, IProductRepository
{
    public ProductRepository(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IRestToLinqParser<Product> parser,
        ILogger<IRepositoryWithIntId<AppDbContext, Product>> logger)
        : base(dbContextFactory, parser, logger)
    {
    }

    public async Task<IEnumerable<Product>> GetLowStockProducts(int threshold)
    {
        return await Task.FromResult(
            dbset.Where(p => p.Stock < threshold && !p.IsDeleted)
        );
    }

    public async Task<IEnumerable<Product>> GetProductsByCategory(string category)
    {
        return await Task.FromResult(
            dbset.Where(p => p.Category == category && !p.IsDeleted)
        );
    }

    public async Task<decimal> GetAveragePriceByCategory(string category)
    {
        return await dbset
            .Where(p => p.Category == category && !p.IsDeleted)
            .AverageAsync(p => p.Price);
    }
}
```

---

## Data Services

### Available Methods

```csharp
public interface IDataService<DBC, T>
{
    Task<T> Add(T model);
    Task<T> Update(T model);
    Task<bool> Delete(Expression<Func<T, bool>> where);
    ApiResult<T> Search(string restQuery);
}

// With ID-specific methods
public interface IDataServiceWithIntId<DBC, T>
{
    // All IDataService methods plus:
    Task<T> GetById(int id);
}
```

### Custom Data Service Methods

```csharp
public interface IProductService : IDataServiceWithIntId<AppDbContext, Product>
{
    // Business logic methods
    Task<bool> AdjustStock(int productId, int quantity);
    Task<IEnumerable<Product>> GetFeaturedProducts();
    Task<bool> DiscontinueProduct(int productId);
    Task<decimal> CalculateTotalInventoryValue();
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
        try
        {
            logger.LogInformation($"Adjusting stock for product {productId} by {quantity}");
            
            var product = await repository.GetById(productId);
            if (product == null)
                return false;

            product.Stock += quantity;
            
            if (product.Stock < 0)
                throw new InvalidOperationException("Insufficient stock");

            await repository.Update(product);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error adjusting stock for product {productId}");
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetFeaturedProducts()
    {
        return await repository.GetLowStockProducts(10);
    }

    public async Task<bool> DiscontinueProduct(int productId)
    {
        var product = await repository.GetById(productId);
        if (product == null)
            return false;

        product.IsDeleted = true;
        await repository.Update(product);
        return true;
    }

    public async Task<decimal> CalculateTotalInventoryValue()
    {
        var products = repository.GetAll();
        return await products
            .Where(p => !p.IsDeleted)
            .SumAsync(p => p.Price * p.Stock);
    }
}
```

---

## Database Factory & Unit of Work

### Database Factory

Implement `IDatabaseFactory` for DbContext management:

```csharp
using Core.Common;
using Core.Common.Contracts;
using Microsoft.EntityFrameworkCore;

public interface IAppDatabaseFactory : IDatabaseFactory<AppDbContext>
{
}

public class AppDatabaseFactory : BaseDatabaseFactory<AppDbContext>, IAppDatabaseFactory
{
    public AppDatabaseFactory(IDbContextFactory<AppDbContext> contextFactory)
        : base(contextFactory)
    {
    }
}

// Register in DI
builder.Services.AddScoped<IAppDatabaseFactory, AppDatabaseFactory>();
```

### Unit of Work

Implement `IUnitOfWork` for transaction management:

```csharp
using Core.Common.Contracts;
using Microsoft.EntityFrameworkCore;

public interface IAppUnitOfWork : IUnitOfWork<AppDbContext>
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
}

public class AppUnitOfWork : IAppUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;

    public AppUnitOfWork(
        IAppDatabaseFactory databaseFactory,
        IProductRepository productRepository,
        IOrderRepository orderRepository)
    {
        _context = databaseFactory.Get();
        _productRepository = productRepository;
        _orderRepository = orderRepository;
    }

    public IProductRepository Products => _productRepository;
    public IOrderRepository Orders => _orderRepository;

    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

// Usage
public class OrderService
{
    private readonly IAppUnitOfWork _unitOfWork;

    public OrderService(IAppUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Order> CreateOrderWithStockUpdate(Order order, int productId, int quantity)
    {
        // Add order
        var createdOrder = await _unitOfWork.Orders.Add(order, commit: false);

        // Update product stock
        var product = await _unitOfWork.Products.GetById(productId);
        product.Stock -= quantity;
        await _unitOfWork.Products.Update(product, commit: false);

        // Commit both changes together
        await _unitOfWork.CommitAsync();

        return createdOrder;
    }
}
```

---

## Data Models

### Base Model Properties

All base models include:

```csharp
public abstract class BaseModel
{
    public bool IsDeleted { get; set; }        // Soft delete flag
    public DateTime Created { get; set; }      // Creation timestamp
    public string CreatedBy { get; set; }      // Creator identifier
    public DateTime? LastUpdated { get; set; } // Last update timestamp
    public string LastUpdatedBy { get; set; }  // Last updater identifier
}
```

### ID-Specific Models

```csharp
// Integer ID
public class BaseModelWithIntId : BaseModel, IModelWithId
{
    public int Id { get; set; }
}

// GUID ID
public class BaseModelWithGuidId : BaseModel, IModelWithGuid
{
    public Guid Id { get; set; } = Guid.NewGuid();
}

// String ID
public class BaseModelWithStringId : BaseModel, IModelWithStringId
{
    public string Id { get; set; }
}
```

### Lookup Models

For simple lookup/reference data:

```csharp
public class BaseLookupModel : BaseModelWithIntId, ILookupModel
{
    public string Code { get; set; }
    public string Description { get; set; }
}

// Example usage
public class Category : BaseLookupModel
{
    // Inherits: Id, Code, Description, IsDeleted, Created, etc.
}
```

### Custom Models

```csharp
public class Product : BaseModelWithIntId
{
    public string Name { get; set; }
    public string SKU { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    
    // Navigation property
    public Category Category { get; set; }
}

public class Order : BaseModelWithGuidId
{
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    
    // Collection navigation
    public List<OrderItem> Items { get; set; }
}

public class OrderItem : BaseModelWithIntId
{
    public Guid OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    // Navigation properties
    public Order Order { get; set; }
    public Product Product { get; set; }
}
```

---

## REST Query Integration

SilverCodeAPI integrates seamlessly with REST-Parser. See [Rest-Parser-Usage.md](Rest-Parser-Usage.md) for full query syntax.

### Basic Queries

```csharp
// Simple search
var result = productService.Search("category=Electronics");

// With operators
var result = productService.Search("price[lt]=1000&stock[gt]=0");

// With sorting
var result = productService.Search("category=Electronics&$sort_by=price[ASC]");

// With pagination
var result = productService.Search("isActive=true&$page=1&$pagesize=20");
```

### ApiResult Structure

```csharp
public class ApiResult<T>
{
    public IEnumerable<T> Data { get; set; }
    public Pagination? Pagination { get; set; }
}

public class Pagination
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int PageCount { get; set; }
    public int TotalCount { get; set; }
}
```

### Controller Example

```csharp
[HttpGet]
public IActionResult Search([FromQuery] string q = "")
{
    try
    {
        var result = _productService.Search(q ?? "$pagesize=20");
        
        var response = new
        {
            data = result.Data,
            page = result.Pagination?.PageNumber ?? 1,
            pageSize = result.Pagination?.PageSize ?? result.Data.Count(),
            totalCount = result.Pagination?.TotalCount ?? result.Data.Count(),
            totalPages = result.Pagination?.PageCount ?? 1
        };
        
        return Ok(response);
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
}
```

### Query Examples

```http
# Get all electronics under $1000, sorted by price
GET /api/products?category=Electronics&price[lt]=1000&$sort_by=price[ASC]

# Get page 2 of active products with stock
GET /api/products?isActive=true&stock[gt]=0&$page=2&$pagesize=25

# Search by name containing "Pro"
GET /api/products?name[contains]=Pro&$sort_by=price[DESC]

# Complex query with multiple conditions
GET /api/products?category=Electronics&price[ge]=100&price[le]=500&stock[gt]=5&$sort_by=name[ASC]&$page=1&$pagesize=10
```

---

## Auditing

### Built-in Audit Fields

All entities automatically track:

```csharp
public class Product : BaseModelWithIntId
{
    // Your properties
    public string Name { get; set; }
    
    // Inherited audit properties:
    // - DateTime Created
    // - string CreatedBy
    // - DateTime? LastUpdated
    // - string LastUpdatedBy
    // - bool IsDeleted
}
```

### Custom Auditor

Implement `IAuditor` to populate audit fields:

```csharp
using Core.Common;
using Core.Common.Contracts;
using Microsoft.AspNetCore.Http;

public class UserAuditor : BaseAuditor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserAuditor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override string GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
    }
}

// Register in DI
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditor, UserAuditor>();
```

### Using Auditor in Repository

```csharp
public class ProductRepository : BaseRepositoryWithIntId<AppDbContext, Product>
{
    private readonly IAuditor _auditor;

    public ProductRepository(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IRestToLinqParser<Product> parser,
        ILogger<IRepositoryWithIntId<AppDbContext, Product>> logger,
        IAuditor auditor)
        : base(dbContextFactory, parser, logger)
    {
        _auditor = auditor;
    }

    public override async Task<Product> Add(Product entity, bool commit = true)
    {
        entity.CreatedBy = _auditor.GetCurrentUser();
        entity.Created = DateTime.UtcNow;
        
        return await base.Add(entity, commit);
    }

    public override async Task<Product> Update(Product entity, bool commit = true)
    {
        entity.LastUpdatedBy = _auditor.GetCurrentUser();
        entity.LastUpdated = DateTime.UtcNow;
        
        return await base.Update(entity, commit);
    }
}
```

### Soft Delete

```csharp
// Instead of hard delete, use soft delete
public async Task<bool> SoftDelete(int productId)
{
    var product = await repository.GetById(productId);
    if (product == null)
        return false;

    product.IsDeleted = true;
    product.LastUpdatedBy = _auditor.GetCurrentUser();
    product.LastUpdated = DateTime.UtcNow;
    
    await repository.Update(product);
    return true;
}

// Filter out deleted items in queries
public IQueryable<Product> GetActiveProducts()
{
    return repository.GetAll().Where(p => !p.IsDeleted);
}
```

---

## Complete Examples

### E-Commerce Product Management

```csharp
// Models
public class Product : BaseModelWithIntId
{
    public string SKU { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public bool IsFeatured { get; set; }
    
    public Category Category { get; set; }
}

public class Category : BaseLookupModel
{
    public List<Product> Products { get; set; }
}

// Repository
public interface IProductRepository : IRepositoryWithIntId<AppDbContext, Product>
{
    Task<IEnumerable<Product>> GetFeaturedProducts();
    Task<IEnumerable<Product>> GetLowStockProducts(int threshold);
}

public class ProductRepository : BaseRepositoryWithIntId<AppDbContext, Product>, IProductRepository
{
    public ProductRepository(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IRestToLinqParser<Product> parser,
        ILogger<IRepositoryWithIntId<AppDbContext, Product>> logger)
        : base(dbContextFactory, parser, logger)
    {
    }

    public async Task<IEnumerable<Product>> GetFeaturedProducts()
    {
        return await Task.FromResult(
            dbset.Where(p => p.IsFeatured && !p.IsDeleted)
                .Include(p => p.Category)
        );
    }

    public async Task<IEnumerable<Product>> GetLowStockProducts(int threshold)
    {
        return await Task.FromResult(
            dbset.Where(p => p.Stock < threshold && !p.IsDeleted)
        );
    }
}

// Service
public interface IProductService : IDataServiceWithIntId<AppDbContext, Product>
{
    Task<IEnumerable<Product>> GetFeaturedProducts();
    Task<bool> UpdateStock(int productId, int newStock);
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

    public async Task<IEnumerable<Product>> GetFeaturedProducts()
    {
        return await repository.GetFeaturedProducts();
    }

    public async Task<bool> UpdateStock(int productId, int newStock)
    {
        var product = await repository.GetById(productId);
        if (product == null)
            return false;

        product.Stock = newStock;
        await repository.Update(product);
        return true;
    }

    public async Task<IEnumerable<Product>> GetProductsNeedingRestock(int threshold)
    {
        return await repository.GetLowStockProducts(threshold);
    }
}

// Controller
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public IActionResult Search([FromQuery] string q = "")
    {
        var result = _productService.Search(q ?? "$pagesize=20");
        return Ok(new { data = result.Data, pagination = result.Pagination });
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        var products = await _productService.GetFeaturedProducts();
        return Ok(products);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10)
    {
        var products = await _productService.GetProductsNeedingRestock(threshold);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var product = await _productService.GetById(id);
        return product != null ? Ok(product) : NotFound();
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
        if (id != product.Id)
            return BadRequest();

        var updated = await _productService.Update(product);
        return Ok(updated);
    }

    [HttpPatch("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] int stock)
    {
        var success = await _productService.UpdateStock(id, stock);
        return success ? NoContent() : NotFound();
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

## Best Practices

### 1. Use DbContextFactory

Always use `IDbContextFactory<T>` for proper DbContext lifecycle management:

```csharp
// ✅ GOOD
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ❌ AVOID
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
```

### 2. Prefer UTC Dates

```csharp
// ✅ GOOD
entity.Created = DateTime.UtcNow;
entity.LastUpdated = DateTime.UtcNow;

// ❌ AVOID
entity.Created = DateTime.Now;
```

### 3. Use Async Properly

```csharp
// ✅ GOOD
public async Task<Product> GetProductAsync(int id)
{
    return await repository.GetById(id);
}

// ❌ AVOID
public Product GetProduct(int id)
{
    return repository.GetById(id).Result;
}
```

### 4. Implement Read-Only Repositories

```csharp
// For queries that don't need write access
public class ProductQueryService
{
    private readonly IReadRepositoryWithIntId<AppDbContext, Product> _readOnlyRepo;

    public ProductQueryService(IReadRepositoryWithIntId<AppDbContext, Product> readOnlyRepo)
    {
        _readOnlyRepo = readOnlyRepo;
    }
}
```

### 5. Use Unit of Work for Transactions

```csharp
public async Task<Order> CreateOrderWithInventoryUpdate(CreateOrderDto dto)
{
    using var unitOfWork = _unitOfWorkFactory.Create();
    
    try
    {
        // Create order
        var order = await unitOfWork.Orders.Add(orderEntity, commit: false);
        
        // Update inventory
        foreach (var item in dto.Items)
        {
            var product = await unitOfWork.Products.GetById(item.ProductId);
            product.Stock -= item.Quantity;
            await unitOfWork.Products.Update(product, commit: false);
        }
        
        // Commit all changes together
        await unitOfWork.CommitAsync();
        
        return order;
    }
    catch
    {
        // Rollback is automatic
        throw;
    }
}
```

### 6. Leverage REST Query Validation

```csharp
[HttpGet]
public IActionResult Search([FromQuery] string q = "")
{
    try
    {
        // Provide sensible defaults
        if (string.IsNullOrWhiteSpace(q))
            q = "$sort_by=Id&$pagesize=20";

        var result = _service.Search(q);
        return Ok(result);
    }
    catch (REST_InvalidFieldnameException ex)
    {
        return BadRequest(new { error = "Invalid field", field = ex.Message });
    }
    catch (REST_InvalidOperatorException ex)
    {
        return BadRequest(new { error = "Invalid operator", details = ex.Message });
    }
    catch (REST_InvalidValueException ex)
    {
        return BadRequest(new { error = "Invalid value", details = ex.Message });
    }
}
```

### 7. Use DTOs for API Responses

```csharp
// Don't expose entities directly
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    // Exclude: CreatedBy, LastUpdatedBy, IsDeleted
}

[HttpGet("{id}")]
public async Task<IActionResult> Get(int id)
{
    var product = await _productService.GetById(id);
    if (product == null)
        return NotFound();

    var dto = new ProductDto
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price,
        Stock = product.Stock
    };
    
    return Ok(dto);
}
```

### 8. Implement Proper Logging

```csharp
public class ProductService : BaseDataServiceWithIntId<AppDbContext, Product>
{
    public override async Task<Product> Add(Product model)
    {
        try
        {
            logger.LogInformation("Adding new product: {ProductName}", model.Name);
            var result = await base.Add(model);
            logger.LogInformation("Successfully added product with ID: {ProductId}", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding product: {ProductName}", model.Name);
            throw;
        }
    }
}
```

---

## Migration Guide

### From Direct EF Core to SilverCodeAPI

#### Before (Direct EF Core)

```csharp
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return Ok(product);
    }
}
```

#### After (SilverCodeAPI)

```csharp
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public IActionResult GetProducts([FromQuery] string q = "")
    {
        var result = _productService.Search(q ?? "$pagesize=20");
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        var created = await _productService.Add(product);
        return Ok(created);
    }
}
```

### Migration Steps

1. **Add NuGet Packages**
2. **Update Entity Models** - Inherit from appropriate base models
3. **Create Repositories** - Implement repository interfaces
4. **Create Data Services** - Implement service layer
5. **Update DI Registration** - Register new services
6. **Update Controllers** - Inject services instead of DbContext
7. **Update Queries** - Use REST query syntax

---

## Troubleshooting

### Common Issues

#### Issue: "DbContext has been disposed"

**Cause**: DbContext lifecycle mismatch

**Solution**: Ensure you're using `IDbContextFactory<T>`:

```csharp
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
```

#### Issue: "REST parser not found"

**Cause**: REST parser not registered for entity type

**Solution**: Register parser in DI:

```csharp
builder.Services.RegisterRestParser<Product>();
```

#### Issue: "Navigation properties are null"

**Cause**: Include not specified

**Solution**: Use `includeChildren` parameter:

```csharp
var product = await repository.GetById(id, includeChildren: true);
```

#### Issue: "Audit fields are null"

**Cause**: Auditor not implemented or registered

**Solution**: Implement and register auditor:

```csharp
builder.Services.AddScoped<IAuditor, UserAuditor>();
```

#### Issue: "Pagination not working"

**Cause**: Missing pagination parameters in query

**Solution**: Include `$page` and `$pagesize`:

```csharp
var result = service.Search("category=Electronics&$page=1&$pagesize=20");
```

---

## Additional Resources

- **REST-Parser Documentation**: [Rest-Parser-Usage.md](Rest-Parser-Usage.md)
- **GitHub Repository**: https://github.com/BigBadJock/SilverCodeAPI
- **NuGet Packages**: 
  - https://www.nuget.org/packages/Core.Common.Contracts/
  - https://www.nuget.org/packages/Core.Common.DataModels/
  - https://www.nuget.org/packages/Core.Common/

---

**Last Updated**: 2025  
**Library Version**: 1.2025.*  
**Target Framework**: .NET 10  
**Author**: John McArthur
