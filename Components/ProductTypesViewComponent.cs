using Intex.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;

namespace Intex.Components
{
    public class ProductTypesViewComponent : ViewComponent
    {
        private IProductRepository _repo;
        public ProductTypesViewComponent(IProductRepository temp) 
        { 
            _repo = temp;
        }
        public IViewComponentResult Invoke()
        {
            ViewBag.SelectedProductType = RouteData?.Values["productType"];

            var productType = _repo.Categories
                .Select(x => x.name)
                .Distinct()
                .OrderBy(x => x);

            return View(productType);
        }

    }
}
