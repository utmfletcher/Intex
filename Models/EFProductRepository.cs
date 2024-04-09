using Intex.Data;

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
    }
}
