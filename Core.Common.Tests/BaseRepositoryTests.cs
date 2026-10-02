using Core.Common.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Core.Common.Tests
{
    public class BaseRepositoryTests : IDisposable
    {
        private readonly TestDatabase db = new();

        public void Dispose() => db.Dispose();

        private async Task<int> SeedProduct(string name = "Widget", DateTime? created = null, string? createdBy = null)
        {
            using var ctx = db.NewContext();
            var product = new Product { Name = name, Created = created ?? DateTime.UtcNow, CreatedBy = createdBy };
            ctx.Products.Add(product);
            await ctx.SaveChangesAsync();
            db.SaveCounter.Saves.Clear();
            return product.Id;
        }

        #region Add

        [Fact]
        public async Task Add_SetsCreatedAndLastUpdated_AndPersists()
        {
            var repo = db.Get<ProductRepository>();
            var before = DateTime.UtcNow;

            var added = await repo.Add(new Product { Name = "Widget", Created = new DateTime(2000, 1, 1) });

            Assert.True(added.Id > 0);
            Assert.InRange(added.Created, before, DateTime.UtcNow);
            Assert.NotNull(added.LastUpdated);
            using var ctx = db.NewContext();
            Assert.Equal("Widget", ctx.Products.Single().Name);
        }

        [Fact]
        public async Task Add_Null_Throws()
        {
            var repo = db.Get<ProductRepository>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.Add(null!));
        }

        [Fact]
        public async Task Add_WithoutCommit_IsNotSavedUntilCommit()
        {
            var repo = db.Get<ProductRepository>();

            await repo.Add(new Product { Name = "A" }, commit: false);
            await repo.Add(new Product { Name = "B" }, commit: false);

            using (var ctx = db.NewContext())
            {
                Assert.Empty(ctx.Products);
            }

            await repo.Commit();

            using (var ctx = db.NewContext())
            {
                Assert.Equal(2, ctx.Products.Count());
            }
            Assert.Equal([2], db.SaveCounter.Saves);
        }

        #endregion

        #region AddBatch

        [Fact]
        public async Task AddBatch_CommitsEveryBatchSize_AndRemainderAtEnd()
        {
            var repo = db.Get<ProductRepository>();
            var reports = new List<ProgressReport>();

            await repo.AddBatch(Enumerable.Range(1, 10).Select(i => new Product { Name = $"P{i}" }), 4, new SyncProgress(reports.Add));

            Assert.Equal([4, 4, 2], db.SaveCounter.Saves);
            Assert.Equal(Enumerable.Range(1, 10), reports.Select(r => r.CurrentProgress));
            Assert.All(reports, r => Assert.Equal(10, r.TotalProgress));
            Assert.All(reports, r => Assert.Equal("Saving 10 Product", r.Message));
        }

        [Fact]
        public async Task AddBatch_ExactMultipleOfBatchSize_DoesNotCommitTwiceAtEnd()
        {
            var repo = db.Get<ProductRepository>();

            await repo.AddBatch(Enumerable.Range(1, 6).Select(i => new Product { Name = $"P{i}" }), 3, null);

            Assert.Equal([3, 3], db.SaveCounter.Saves);
        }

        [Fact]
        public async Task AddBatch_NullProgress_IsAllowed()
        {
            var repo = db.Get<ProductRepository>();

            await repo.AddBatch([new Product { Name = "A" }], 5, null);

            using var ctx = db.NewContext();
            Assert.Single(ctx.Products);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task AddBatch_InvalidBatchSize_Throws(int batchSize)
        {
            var repo = db.Get<ProductRepository>();

            await Assert.ThrowsAsync<ArgumentException>(() => repo.AddBatch([new Product()], batchSize, null));
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_DetachedEntity_SavesChanges_SetsLastUpdated_PreservesCreated()
        {
            var created = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var id = await SeedProduct("Old", created, "alice");
            var repo = db.Get<ProductRepository>();

            await repo.Update(new Product { Id = id, Name = "New", Created = new DateTime(2000, 1, 1), CreatedBy = "mallory" });

            using var ctx = db.NewContext();
            var saved = ctx.Products.Single();
            Assert.Equal("New", saved.Name);
            Assert.Equal(created, saved.Created);
            Assert.Equal("alice", saved.CreatedBy);
            Assert.NotNull(saved.LastUpdated);
            Assert.True(saved.LastUpdated > created);
        }

        [Fact]
        public async Task Update_TrackedEntity_SavesChanges()
        {
            var id = await SeedProduct("Old");
            var repo = db.Get<ProductRepository>();
            var product = await repo.GetById(id);

            product!.Name = "New";
            await repo.Update(product);

            using var ctx = db.NewContext();
            Assert.Equal("New", ctx.Products.Single().Name);
        }

        [Fact]
        public async Task Update_Null_Throws()
        {
            var repo = db.Get<ProductRepository>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.Update(null!));
        }

        #endregion

        #region Delete

        [Fact]
        public async Task DeleteById_Existing_ReturnsTrue_AndRemoves()
        {
            var id = await SeedProduct();
            var repo = db.Get<ProductRepository>();

            Assert.True(await repo.Delete(id));

            using var ctx = db.NewContext();
            Assert.Empty(ctx.Products);
        }

        [Fact]
        public async Task DeleteById_Missing_ReturnsFalse()
        {
            var repo = db.Get<ProductRepository>();

            Assert.False(await repo.Delete(12345));
        }

        [Fact]
        public async Task DeleteWhere_Matches_ReturnsTrue_AndRemovesOnlyMatches()
        {
            await SeedProduct("keep");
            await SeedProduct("drop1");
            await SeedProduct("drop2");
            var repo = db.Get<ProductRepository>();

            Assert.True(await repo.Delete(p => p.Name.StartsWith("drop")));

            using var ctx = db.NewContext();
            Assert.Equal("keep", ctx.Products.Single().Name);
            Assert.Equal([2], db.SaveCounter.Saves);
        }

        [Fact]
        public async Task DeleteWhere_NoMatches_ReturnsFalse()
        {
            await SeedProduct();
            var repo = db.Get<ProductRepository>();

            Assert.False(await repo.Delete(p => p.Name == "nope"));
            Assert.Empty(db.SaveCounter.Saves);
        }

        [Fact]
        public async Task DeleteEntity_RemovesIt()
        {
            var id = await SeedProduct();
            var repo = db.Get<ProductRepository>();
            var product = await repo.GetById(id);

            Assert.True(await repo.Delete(product!));

            using var ctx = db.NewContext();
            Assert.Empty(ctx.Products);
        }

        [Fact]
        public async Task Delete_WithoutCommit_IsNotSavedUntilCommit()
        {
            var id = await SeedProduct();
            var repo = db.Get<ProductRepository>();

            await repo.Delete(id, commit: false);
            using (var ctx = db.NewContext())
            {
                Assert.Single(ctx.Products);
            }

            await repo.Commit();
            using (var ctx = db.NewContext())
            {
                Assert.Empty(ctx.Products);
            }
        }

        #endregion

        #region Guid and string ids

        [Fact]
        public async Task GuidRepository_AddGetDelete()
        {
            var repo = db.Get<OrderRepository>();

            var order = await repo.Add(new Order { OrderNumber = "ORD-1" });

            Assert.NotEqual(Guid.Empty, order.Id);
            Assert.Equal("ORD-1", (await repo.GetById(order.Id))?.OrderNumber);
            Assert.Null(await repo.GetById(Guid.NewGuid()));
            Assert.True(await repo.Delete(order.Id));
            Assert.False(await repo.Delete(order.Id));
        }

        [Fact]
        public async Task StringRepository_AddGetDelete()
        {
            var repo = db.Get<TagRepository>();

            await repo.Add(new Tag { Id = "dotnet", Label = ".NET" });

            Assert.Equal(".NET", (await repo.GetById("dotnet"))?.Label);
            Assert.Null(await repo.GetById("missing"));
            Assert.True(await repo.Delete("dotnet"));
            Assert.False(await repo.Delete("dotnet"));
        }

        [Fact]
        public async Task StringRepository_DeleteNullId_Throws()
        {
            var repo = db.Get<TagRepository>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.Delete((string)null!));
        }

        #endregion

        #region Disposal

        [Fact]
        public void Repository_DisposedWithScope_DisposesDbContext()
        {
            ProductRepository repo;
            using (var scope = db.CreateScope())
            {
                repo = (ProductRepository)scope.ServiceProvider.GetService(typeof(ProductRepository))!;
            }

            Assert.Throws<ObjectDisposedException>(() => repo.GetAll().ToList());
        }

        [Fact]
        public async Task Repository_DisposedWithAsyncScope_DisposesDbContext()
        {
            ProductRepository repo;
            await using (var scope = db.CreateAsyncScope())
            {
                repo = (ProductRepository)scope.ServiceProvider.GetService(typeof(ProductRepository))!;
            }

            Assert.Throws<ObjectDisposedException>(() => repo.GetAll().ToList());
        }

        [Fact]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var repo = db.Get<ProductRepository>();

            repo.Dispose();
            repo.Dispose();
        }

        #endregion

        private sealed class SyncProgress(Action<ProgressReport> report) : IProgress<ProgressReport>
        {
            public void Report(ProgressReport value) => report(value);
        }
    }
}
