namespace Intex.Models.ViewModels;

public class CleanProductViewModel
{
    public int ProductId { get; set; }
    public string? Name { get; set; }
    public int? Year { get; set; }
    public int? NumParts { get; set; }
    public int? Price { get; set; }
    public string? ImgLink { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? Description { get; set; }
    public List<string> CategoryNames { get; set; } = new List<string>();  // To hold the names of the categories
}
