namespace Intex.Models
{
    public interface IProductRepository
    {
        public IQueryable<Product> Products { get; }
        public IQueryable<CleanProduct> CleanProducts { get;}

        public IQueryable<CategoryClean> Categories { get; }

        public IQueryable<ProductCategoryClean> ProductCategories { get; }
    }
}
