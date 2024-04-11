namespace Intex.Models
{
    public interface IProductRepository
    {
        public IQueryable<Product> Products { get; }
        public IQueryable<CleanProduct> CleanProducts { get;}

        public IQueryable<CategoryClean> Categories { get; }

        public IQueryable<ProductCategoryClean> ProductCategories { get; }

        public IQueryable<top_20_product> top_20_products  { get; }
        public IQueryable<Order> Orders { get; }

    }
}
