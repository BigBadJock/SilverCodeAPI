using Core.Common.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Core.Common.Tests
{
    public class BaseDataServiceTests : IDisposable
    {
        private readonly TestDatabase db = new();

        public void Dispose() => db.Dispose();

        [Fact]
        public async Task IntService_CrudThroughRepository()
        {
            var service = db.Get<ProductService>();

            var added = await service.Add(new Product { Name = "Widget", Price = 5m });
            Assert.Equal("Widget", (await service.GetById(added.Id))?.Name);

            added.Price = 6m;
            await service.Update(added);
            using (var ctx = db.NewContext())
            {
                Assert.Equal(6m, ctx.Products.Single().Price);
            }

            Assert.True(await service.Delete(added.Id));
            Assert.Null(await service.GetById(added.Id));
        }

        [Fact]
        public async Task Search_And_GetAll_DelegateToRepository()
        {
            var service = db.Get<ProductService>();
            await service.Add(new Product { Name = "A", Price = 1m });
            await service.Add(new Product { Name = "B", Price = 2m });

            Assert.Equal(["B"], service.Search("price[gt]=1").Data.Select(p => p.Name));
            Assert.Equal(2, service.GetAll().Count());
        }

        [Fact]
        public async Task DeleteWhere_DelegatesToRepository()
        {
            var service = db.Get<ProductService>();
            await service.Add(new Product { Name = "A" });

            Assert.False(await service.Delete(p => p.Name == "nope"));
            Assert.True(await service.Delete(p => p.Name == "A"));
        }

        [Fact]
        public async Task GuidService_GetByIdAndDelete()
        {
            var service = db.Get<OrderService>();
            var order = await service.Add(new Order { OrderNumber = "ORD-1" });

            Assert.Equal("ORD-1", (await service.GetById(order.Id))?.OrderNumber);
            Assert.True(await service.Delete(order.Id));
            Assert.False(await service.Delete(order.Id));
        }

        [Fact]
        public async Task StringService_GetByIdAndDelete()
        {
            var service = db.Get<TagService>();
            await service.Add(new Tag { Id = "a", Label = "A" });

            Assert.Equal("A", (await service.GetById("a"))?.Label);
            Assert.True(await service.Delete("a"));
            Assert.Null(await service.GetById("a"));
        }

        [Fact]
        public async Task RepositoryExceptions_AreRethrown()
        {
            var repo = new Mock<IRepositoryWithIntId<TestDbContext, Product>>();
            var failure = new DbUpdateException("boom");
            repo.Setup(r => r.Add(It.IsAny<Product>(), It.IsAny<bool>())).ThrowsAsync(failure);
            repo.Setup(r => r.Update(It.IsAny<Product>(), It.IsAny<bool>())).ThrowsAsync(failure);
            repo.Setup(r => r.GetById(It.IsAny<int>())).ThrowsAsync(failure);
            repo.Setup(r => r.Delete(It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(failure);
            repo.Setup(r => r.GetAll(It.IsAny<string>())).Throws(new ArgumentException("bad query"));
            var service = new ProductService(repo.Object, NullLogger<IDataServiceWithIntId<TestDbContext, Product>>.Instance);

            Assert.Same(failure, await Assert.ThrowsAsync<DbUpdateException>(() => service.Add(new Product())));
            Assert.Same(failure, await Assert.ThrowsAsync<DbUpdateException>(() => service.Update(new Product())));
            Assert.Same(failure, await Assert.ThrowsAsync<DbUpdateException>(() => service.GetById(1)));
            Assert.Same(failure, await Assert.ThrowsAsync<DbUpdateException>(() => service.Delete(1)));
            Assert.Throws<ArgumentException>(() => service.Search("x"));
        }

        [Fact]
        public async Task DeleteById_PassesCommitFlag()
        {
            var repo = new Mock<IRepositoryWithIntId<TestDbContext, Product>>();
            repo.Setup(r => r.Delete(7, false)).ReturnsAsync(true);
            var service = new ProductService(repo.Object, NullLogger<IDataServiceWithIntId<TestDbContext, Product>>.Instance);

            Assert.True(await service.Delete(7, commit: false));
            repo.Verify(r => r.Delete(7, false), Times.Once);
        }
    }
}
