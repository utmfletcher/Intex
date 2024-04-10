
using Intex.Models;
using Intex.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Construction;
using Microsoft.EntityFrameworkCore;
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

        public IActionResult Index(int pageNum, string? productType)
        {



            int pageSize = 5;
            pageNum = Math.Max(1, pageNum); // Ensure pageNum is at least 1

            // Using LINQ method syntax to join and filter data
            var query = _repo.CleanProducts
                .Join(_repo.ProductCategories,
                      product => product.product_id,
                      productCategory => productCategory.p_id,
                      (product, productCategory) => new { product, productCategory })
                .Join(_repo.Categories,
                      combined => combined.productCategory.c_id,
                      category => category.category_id,
                      (combined, category) => new { combined.product, category })
                .Where(combined => productType == null || combined.category.name == productType)
                .OrderBy(combined => combined.product.name)
                .Select(combined => new CleanProduct
                {
                    product_id = combined.product.product_id,
                    name = combined.product.name,
                    year = combined.product.year,
                    num_parts = combined.product.num_parts,
                    price = combined.product.price,
                    img_link = combined.product.img_link,
                    primary_color = combined.product.primary_color,
                    secondary_color = combined.product.secondary_color,
                    description = combined.product.description
                });

            var totalItems = query.Count();

            var products = query
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToList(); // Materialize the query to execute it

            var setup = new ProductListViewModel
            {
                CleanProducts = products,
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = totalItems
                },
                CurrentProductType = productType
            };

            // Pass the viewModel to the "ProductDisplay" view
            return View(setup);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secrets()
        {
            return View();
        }



        //public IActionResult ProductDisplay(int pageNum, string? productType)    
        //{
        //    int pageSize = 5;
        //    var setup = new ProductListViewModel
        //    {
        //        Products = _repo.Products
        //        .Where(x => x.Category == productType || productType == null)
        //        .OrderBy(x => x.Name)
        //        .Skip(pageSize * (pageNum - 1))
        //        .Take(pageSize),

        //        PaginationInfo = new PaginationInfo
        //        {
        //            CurrentPage = pageNum,
        //            ItemsPerPage = pageSize,
        //            TotalItems = productType == null ? _repo.Products.Count() : _repo.Products.Where(x => x.Category == productType).Count()
        //        },

        //        CurrentProductType = productType
        //    };


        //    // Pass the viewModel to the "ProductDisplay" view
        //    return View(setup);
        //}

        public IActionResult ProductDisplay(int pageNum, string? productType)
        {
      

            int pageSize = 5;
            pageNum = Math.Max(1, pageNum); // Ensure pageNum is at least 1

            // Using LINQ method syntax to join and filter data
            var query = _repo.CleanProducts
                .Join(_repo.ProductCategories,
                      product => product.product_id,
                      productCategory => productCategory.p_id,
                      (product, productCategory) => new { product, productCategory })
                .Join(_repo.Categories,
                      combined => combined.productCategory.c_id,
                      category => category.category_id,
                      (combined, category) => new { combined.product, category })
                .Where(combined => productType == null || combined.category.name == productType)
                .OrderBy(combined => combined.product.name)
                .Select(combined => new CleanProduct
                {
                    product_id = combined.product.product_id,
                    name = combined.product.name,
                    year = combined.product.year,
                    num_parts = combined.product.num_parts,
                    price = combined.product.price,
                    img_link = combined.product.img_link,
                    primary_color = combined.product.primary_color,
                    secondary_color = combined.product.secondary_color,
                    description = combined.product.description
                });

            var totalItems = query.Count();

            var products = query
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToList(); // Materialize the query to execute it

            var setup = new ProductListViewModel
            {
                CleanProducts = products,
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = totalItems
                },
                CurrentProductType = productType
            };

            // Pass the viewModel to the "ProductDisplay" view
            return View(setup);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
