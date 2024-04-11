namespace Intex.Models
{
    public interface IProductRepository
    {
        public IQueryable<Product> Products { get; }
        public IQueryable<CleanProduct> CleanProducts { get;}
        void UpdateCleanProduct(CleanProduct product);
        void SaveChanges();

        public IQueryable<CategoryClean> Categories { get; }

        public IQueryable<ProductCategoryClean> ProductCategories { get; }

        public IQueryable<top_20_product> top_20_products  { get; }

        public IQueryable<ItemReccomendation> ItemReccomendations { get; }

        public IQueryable<User6Product> User6Products { get; }

        void DeleteCleanProduct(CleanProduct product);
        void AddCleanProduct(CleanProduct product);





    }
}
