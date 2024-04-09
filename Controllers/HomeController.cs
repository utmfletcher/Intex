using Intex.Models;
using Intex.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Drawing.Printing;

namespace Intex.Controllers
{
    public class HomeController : Controller
    {
       // private readonly ILogger<HomeController> _logger;

        private IProductRepository _repo;

       /* public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }*/
       public HomeController(IProductRepository temp)
        {
            _repo = temp;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult ProductDisplay()
             
        {
            var blah = new ProductListViewModel
            {
                Products = _repo.Products
               //.OrderBy(x => x.ProductId)
               

            };


            // Pass the viewModel to the "ProductDisplay" view
            return View(blah);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
