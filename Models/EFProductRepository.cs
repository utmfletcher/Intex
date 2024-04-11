﻿using Intex.Data;
using Microsoft.EntityFrameworkCore;

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
        public IQueryable<Order> Orders => _context.Orders;
        public void AddOrder(Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
        }

    }

}
