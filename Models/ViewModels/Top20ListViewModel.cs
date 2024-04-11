namespace Intex.Models.ViewModels
{
    public class Top20ListViewModel
    {
        // Use CleanProductViewModel instead of CleanProduct to include category information
        public IEnumerable<Top20ViewModel> Top20ViewModels { get; set; } = new List<Top20ViewModel>();
        public IEnumerable<User6ViewModel> User6ViewModels { get; set; } = new List<User6ViewModel>();

        public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();

    }
}
