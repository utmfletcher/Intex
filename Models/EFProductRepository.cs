using Intex.Data;
using Microsoft.EntityFrameworkCore; // Ensure you have this using directive for EntityState
using System;
using System.Linq;

namespace Intex.Models
{
    public class EFProductRepository : IProductRepository
    {
        private readonly PostgresContext _context;

        public EFProductRepository(PostgresContext context)
        {
            _context = context;
        }

        public IQueryable<Product> Products => _context.Products;

        public IQueryable<CleanProduct> CleanProducts => _context.CleanProducts;

        public IQueryable<CategoryClean> Categories => _context.Categories;

        public IQueryable<ProductCategoryClean> ProductCategories => _context.ProductCategories;

        public IQueryable<top_20_product> top_20_products => _context.top_20_products;

        public IQueryable<ItemReccomendation> ItemReccomendations => _context.ItemReccomendations;

        public IQueryable<User6Product> User6Products => _context.User6Products;

        public IQueryable<Order> Orders => _context.Orders;

        public IQueryable<Lineitem> Lineitems => _context.Lineitems;

        public IQueryable<Customer> Customers => _context.Customers;

        //object IProductRepository.Orders => throw new NotImplementedException();

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

        public void AddCategoryToProduct(ProductCategoryClean productCategory)
        {
            _context.ProductCategories.Add(productCategory);
        }

        public void AddOrder(Order order)
        {
            _context.Orders.Add(order);
        }

        public void AddLineItem(Lineitem lineitem)
        {
            _context.Lineitems.Add(lineitem);
        }

        public void AddCustomer(Customer customer)
        {
            _context.Customers.Add(customer);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
