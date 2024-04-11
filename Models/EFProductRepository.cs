using Intex.Data;
using Microsoft.EntityFrameworkCore; // Ensure you have this using directive for EntityState
using System.Linq;

namespace Intex.Models
{
    public class EFProductRepository : IProductRepository
    {
        private PostgresContext _context;

        public EFProductRepository(PostgresContext temp)
        {
            _context = temp;
        }

        public IQueryable<Product> Products => _context.Products;

        public IQueryable<CleanProduct> CleanProducts => _context.CleanProducts;

        public IQueryable<CategoryClean> Categories => _context.Categories;

        public IQueryable<ProductCategoryClean> ProductCategories => _context.ProductCategories;

        public IQueryable<top_20_product> top_20_products => _context.top_20_products;

        public IQueryable<ItemReccomendation> ItemReccomendations => _context.ItemReccomendations;

        public IQueryable<User6Product> User6Products => _context.User6Products;

        public void UpdateCleanProduct(CleanProduct product)
        {
            _context.Entry(product).State = EntityState.Modified;
        }
        public void DeleteCleanProduct(CleanProduct product)
        {
            _context.CleanProducts.Remove(product);
        }
        public void AddCleanProduct(CleanProduct product)
        {
            _context.CleanProducts.Add(product);
        }


        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }

}
