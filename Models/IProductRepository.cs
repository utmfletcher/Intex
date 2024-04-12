namespace Intex.Models
{
    public interface IProductRepository
    {

        IQueryable<Product> Products { get; }
        IQueryable<CleanProduct> CleanProducts { get; }
        IQueryable<CategoryClean> Categories { get; }
        IQueryable<ProductCategoryClean> ProductCategories { get; }
        IQueryable<top_20_product> top_20_products { get; }
        IQueryable<ItemReccomendation> ItemReccomendations { get; }
        IQueryable<User6Product> User6Products { get; }
        IQueryable<Order> Orders { get; }


        public IQueryable<Lineitem> Lineitems { get; }

        public IQueryable<Customer> Customers { get; }

        //object Orders { get; }

        void UpdateCleanProduct(CleanProduct product);
        void DeleteCleanProduct(CleanProduct product);
        void AddCleanProduct(CleanProduct product);
        void AddOrder(Order order);
        void AddCategoryToProduct(ProductCategoryClean productCategory);

        void SaveChanges();
    }
}
