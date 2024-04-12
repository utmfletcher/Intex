using Intex.Models;
using Microsoft.AspNetCore.Mvc;

namespace Intex.Components
{
    public class ProductColourViewComponent : ViewComponent
    {
        private readonly IProductRepository _repo;

        public ProductColourViewComponent(IProductRepository temp)
        {
            _repo = temp;
        }

        public IViewComponentResult Invoke()
        {
            ViewBag.SelectedColour = RouteData?.Values["Colour"];

            // Fetch the products from the database and materialize the query with ToList()
            var products = _repo.Products.ToList();

            // Process the colors in-memory to get a distinct list
            var colours = products
                .SelectMany(x => new[] { x.PrimaryColor, x.SecondaryColor })
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return View(colours);
        }
    }
}
