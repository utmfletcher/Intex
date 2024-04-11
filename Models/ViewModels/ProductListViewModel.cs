using Intex.Models.ViewModels;

namespace Intex.Models.ViewModels
{
    public class ProductListViewModel
    {
        // Use CleanProductViewModel instead of CleanProduct to include category information
        public IEnumerable<CleanProductViewModel> CleanProducts { get; set; } = new List<CleanProductViewModel>();
        public IEnumerable<Top20ViewModel> Top20ViewModels { get; set; } = new List<Top20ViewModel>();

        public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();

        public string? CurrentProductType { get; set; }

        public string? img_link { get; set;}
    }
}
