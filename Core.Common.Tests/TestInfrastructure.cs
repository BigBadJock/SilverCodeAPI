using Core.Common.Contracts;
using Core.Common.DataModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using REST_Parser;
using REST_Parser.DependencyResolution;

namespace Core.Common.Tests
{
    public class Category : BaseModelWithIntId
    {
        public string Name { get; set; } = string.Empty;
        public List<Product> Products { get; set; } = [];
    }

    public class Product : BaseModelWithIntId
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }

    public class Order : BaseModelWithGuidId
    {
        public string OrderNumber { get; set; } = string.Empty;
    }

    public class Tag : BaseModelWithStringId
    {
        public string Label { get; set; } = string.Empty;
    }

    public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Tag> Tags => Set<Tag>();
    }

    public class CategoryRepository(IDbContextFactory<TestDbContext> f, IRestToLinqParser<Category> p, ILogger<IRepository<TestDbContext, Category>> l)
        : BaseRepositoryWithIntId<TestDbContext, Category>(f, p, l);

    public class ProductRepository(IDbContextFactory<TestDbContext> f, IRestToLinqParser<Product> p, ILogger<IRepository<TestDbContext, Product>> l)
        : BaseRepositoryWithIntId<TestDbContext, Product>(f, p, l);

    public class OrderRepository(IDbContextFactory<TestDbContext> f, IRestToLinqParser<Order> p, ILogger<IRepository<TestDbContext, Order>> l)
        : BaseRepositoryWithGuidId<TestDbContext, Order>(f, p, l);

    public class TagRepository(IDbContextFactory<TestDbContext> f, IRestToLinqParser<Tag> p, ILogger<IRepository<TestDbContext, Tag>> l)
        : BaseRepositoryWithStringId<TestDbContext, Tag>(f, p, l);

    public class ProductReadRepository(IDbContextFactory<TestDbContext> f, IRestToLinqParser<Product> p, ILogger<IReadRepositoryWithIntId<TestDbContext, Product>> l)
        : BaseReadRepositoryWithIntId<TestDbContext, Product>(f, p, l);

    public class ProductService(IRepositoryWithIntId<TestDbContext, Product> r, ILogger<IDataServiceWithIntId<TestDbContext, Product>> l)
        : BaseDataServiceWithIntId<TestDbContext, Product>(r, l);

    public class OrderService(IRepositoryWithGuidId<TestDbContext, Order> r, ILogger<IDataServiceWithGuidId<TestDbContext, Order>> l)
        : BaseDataServiceWithGuidId<TestDbContext, Order>(r, l);

    public class TagService(IRepositoryWithStringId<TestDbContext, Tag> r, ILogger<IDataServiceWithStringId<TestDbContext, Tag>> l)
        : BaseDataServiceWithStringId<TestDbContext, Tag>(r, l);

    /// <summary>
    /// Records the number of rows written by each SaveChanges call
    /// </summary>
    public class SaveCounter : SaveChangesInterceptor
    {
        public List<int> Saves { get; } = [];

        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            Saves.Add(result);
            return ValueTask.FromResult(result);
        }
    }

    /// <summary>
    /// A fresh in-memory SQLite database per test, with the repositories and services wired up through DI
    /// </summary>
    public sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection connection;
        private readonly ServiceProvider provider;

        public SaveCounter SaveCounter { get; } = new();

        public TestDatabase()
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var services = new ServiceCollection();
            services.AddDbContextFactory<TestDbContext>(o => o.UseSqlite(connection).AddInterceptors(SaveCounter));
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
            services.RegisterRestParser<Category>();
            services.RegisterRestParser<Product>();
            services.RegisterRestParser<Order>();
            services.RegisterRestParser<Tag>();
            services.AddScoped<CategoryRepository>();
            services.AddScoped<ProductRepository>();
            services.AddScoped<IRepositoryWithIntId<TestDbContext, Product>>(sp => sp.GetRequiredService<ProductRepository>());
            services.AddScoped<OrderRepository>();
            services.AddScoped<IRepositoryWithGuidId<TestDbContext, Order>>(sp => sp.GetRequiredService<OrderRepository>());
            services.AddScoped<TagRepository>();
            services.AddScoped<IRepositoryWithStringId<TestDbContext, Tag>>(sp => sp.GetRequiredService<TagRepository>());
            services.AddScoped<ProductReadRepository>();
            services.AddScoped<ProductService>();
            services.AddScoped<OrderService>();
            services.AddScoped<TagService>();
            provider = services.BuildServiceProvider();

            using var db = NewContext();
            db.Database.EnsureCreated();
            SaveCounter.Saves.Clear();
        }

        public IServiceScope CreateScope() => provider.CreateScope();

        public AsyncServiceScope CreateAsyncScope() => provider.CreateAsyncScope();

        /// <summary>
        /// Resolves a service in a new scope; the scope is disposed with the database
        /// </summary>
        public T Get<T>() where T : notnull
        {
            var scope = provider.CreateScope();
            scopes.Add(scope);
            return scope.ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// A separate context for seeding and verifying data independently of the repository under test
        /// </summary>
        public TestDbContext NewContext() => provider.GetRequiredService<IDbContextFactory<TestDbContext>>().CreateDbContext();

        private readonly List<IServiceScope> scopes = [];

        public void Dispose()
        {
            scopes.ForEach(s => s.Dispose());
            provider.Dispose();
            connection.Dispose();
        }
    }
}
