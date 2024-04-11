
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
                      (combined, category) => new { combined.product, CategoryName = category.name })
                .GroupBy(combined => combined.product)
                .Select(grouped => new CleanProductViewModel
                {
                    ProductId = grouped.Key.product_id,
                    Name = grouped.Key.name,
                    Year = grouped.Key.year,
                    NumParts = grouped.Key.num_parts,
                    Price = grouped.Key.price,
                    ImgLink = grouped.Key.img_link,
                    PrimaryColor = grouped.Key.primary_color,
                    SecondaryColor = grouped.Key.secondary_color,
                    Description = grouped.Key.description,
                    CategoryNames = grouped.Select(g => g.CategoryName).ToList()
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
        // FUTURE ADMIN PAGE
        //[Authorize(Roles = "Admin")]
        //public IActionResult AdminDashboard()
        //{
        //    // Logic to gather data for the admin dashboard
        //    // This might involve querying databases, preparing view models, etc.

        //    return View();
        //}
        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            // Retrieve all CleanProducts using LINQ.
            var cleanProductsList = _repo.CleanProducts.ToList();

            // Pass the list of products to the view.
            return View(cleanProductsList);
        }

        public IActionResult ProductDisplay(int pageNum, string? productType, int pageSize)
        {
            pageSize = pageSize;

            pageNum = Math.Max(1, pageNum); // Ensure pageNum is at least 1

            var query = _repo.CleanProducts
                .Join(_repo.ProductCategories,
                      product => product.product_id,
                      productCategory => productCategory.p_id,
                      (product, productCategory) => new { product, productCategory })
                .Join(_repo.Categories,
                      combined => combined.productCategory.c_id,
                      category => category.category_id,
                      (combined, category) => new { combined.product, CategoryName = category.name });

            // Apply filtering before grouping
            if (!string.IsNullOrWhiteSpace(productType))
            {
                query = query.Where(combined => combined.CategoryName == productType);
            }

            var groupedQuery = query
                .GroupBy(combined => combined.product)
                .Select(grouped => new CleanProductViewModel
                {
                    ProductId = grouped.Key.product_id,
                    Name = grouped.Key.name,
                    Year = grouped.Key.year,
                    NumParts = grouped.Key.num_parts,
                    Price = grouped.Key.price,
                    ImgLink = grouped.Key.img_link,
                    PrimaryColor = grouped.Key.primary_color,
                    SecondaryColor = grouped.Key.secondary_color,
                    Description = grouped.Key.description,
                    CategoryNames = grouped.Select(g => g.CategoryName).ToList()
                });

            var totalItems = groupedQuery.Count();

            var products = groupedQuery
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

            return View(setup);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
