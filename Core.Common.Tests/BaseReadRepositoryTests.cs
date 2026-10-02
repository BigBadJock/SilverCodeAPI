namespace Core.Common.Tests
{
    public class BaseReadRepositoryTests : IDisposable
    {
        private readonly TestDatabase db = new();

        public BaseReadRepositoryTests()
        {
            using var ctx = db.NewContext();
            var tools = new Category { Name = "Tools" };
            var toys = new Category { Name = "Toys" };
            ctx.Categories.AddRange(tools, toys);
            ctx.Products.AddRange(
                new Product { Name = "Hammer", Price = 15m, Category = tools },
                new Product { Name = "Saw", Price = 25m, Category = tools },
                new Product { Name = "Yo-yo", Price = 3m, Category = toys },
                new Product { Name = "Kite", Price = 12m, Category = toys },
                new Product { Name = "Loose", Price = 1m });
            ctx.SaveChanges();
        }

        public void Dispose() => db.Dispose();

        [Fact]
        public void GetAll_ReturnsAllEntities()
        {
            var repo = db.Get<ProductRepository>();

            Assert.Equal(5, repo.GetAll().Count());
        }

        [Fact]
        public void GetAll_RestQuery_FiltersAndSorts()
        {
            var repo = db.Get<ProductRepository>();

            var result = repo.GetAll("price[ge]=10&$sort_by[desc]=price");

            Assert.Equal(["Saw", "Hammer", "Kite"], result.Data.Select(p => p.Name));
        }

        [Fact]
        public void GetAll_RestQuery_WithoutPaging_HasNoPagination()
        {
            var repo = db.Get<ProductRepository>();

            var result = repo.GetAll("name[contains]=a");

            Assert.Null(result.Pagination);
        }

        [Fact]
        public void GetAll_RestQuery_WithPaging_ReturnsPageAndPagination()
        {
            var repo = db.Get<ProductRepository>();

            var result = repo.GetAll("$sort_by[asc]=price&$page=2&$pagesize=2");

            Assert.Equal(["Kite", "Hammer"], result.Data.Select(p => p.Name));
            Assert.NotNull(result.Pagination);
            Assert.Equal(2, result.Pagination.PageNumber);
            Assert.Equal(2, result.Pagination.PageSize);
            Assert.Equal(3, result.Pagination.PageCount);
            Assert.Equal(5, result.Pagination.TotalCount);
        }

        [Fact]
        public void GetAll_RestQuery_OrAndIn()
        {
            var repo = db.Get<ProductRepository>();

            Assert.Equal(["Kite", "Saw"], repo.GetAll("name=Saw|name=Kite&$sort_by=name").Data.Select(p => p.Name));
            Assert.Equal(["Hammer", "Yo-yo"], repo.GetAll("name[in]=Hammer,Yo-yo&$sort_by=name").Data.Select(p => p.Name));
        }

        [Fact]
        public void GetAll_RestQuery_InvalidField_Throws()
        {
            var repo = db.Get<ProductRepository>();

            Assert.Throws<REST_Parser.Exceptions.REST_InvalidFieldnameException>(() => repo.GetAll("nope=1"));
        }

        [Fact]
        public void GetAll_ChildrenNotIncludedByDefault()
        {
            var repo = db.Get<CategoryRepository>();

            Assert.All(repo.GetAll("$sort_by=name").Data, c => Assert.Empty(c.Products));
        }

        [Fact]
        public void AlwaysIncludeChildren_IncludesCollectionNavigations()
        {
            var repo = db.Get<CategoryRepository>();
            repo.AlwaysIncludeChildren = true;

            var categories = repo.GetAll("$sort_by=name").Data.ToList();

            Assert.Equal(2, categories.Single(c => c.Name == "Tools").Products.Count);
            Assert.Equal(2, categories.Single(c => c.Name == "Toys").Products.Count);
        }

        [Fact]
        public void AlwaysIncludeChildren_IncludesReferenceNavigations()
        {
            var repo = db.Get<ProductRepository>();
            repo.AlwaysIncludeChildren = true;

            var hammer = repo.GetAll("name=Hammer").Data.Single();

            Assert.Equal("Tools", hammer.Category?.Name);
        }

        [Fact]
        public void AlwaysIncludeChildren_AppliesToGetAllQueryable()
        {
            var repo = db.Get<CategoryRepository>();
            repo.AlwaysIncludeChildren = true;

            Assert.All(repo.GetAll().ToList(), c => Assert.Equal(2, c.Products.Count));
        }

        [Fact]
        public async Task ReadRepository_GetById()
        {
            var repo = db.Get<ProductReadRepository>();
            int id;
            using (var ctx = db.NewContext())
            {
                id = ctx.Products.Single(p => p.Name == "Kite").Id;
            }

            Assert.Equal("Kite", (await repo.GetById(id))?.Name);
            Assert.Null(await repo.GetById(-1));
        }

        [Fact]
        public void DbSet_IsExposed()
        {
            var repo = db.Get<ProductRepository>();

            Assert.Equal(5, repo.DbSet.Count());
        }
    }
}
