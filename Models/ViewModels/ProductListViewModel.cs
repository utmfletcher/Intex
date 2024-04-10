using Intex.Models.ViewModels;

namespace Intex.Models.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<CleanProduct> CleanProducts { get; set; }

        public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();
    
        public string? CurrentProductType { get; set; }
    }
}