
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

        public IActionResult Index()
        {



            int pageSize = 20;
         

            var query = _repo.Products
                .Join(_repo.top_20_products,
                      product => product.ProductId,
                      top_20_product => top_20_product.product_ID,
                      (product, top_20_product) => new { product, top_20_product })
                .Select(joinedItem => new Top20ViewModel
                {
                    ProductId = joinedItem.product.ProductId,
                    Name = joinedItem.product.Name,
                    Year = joinedItem.product.Year,
                    NumParts = joinedItem.product.NumParts,
                    Price = joinedItem.product.Price,
                    ImgLink = joinedItem.product.ImgLink,
                    PrimaryColor = joinedItem.product.PrimaryColor,
                    SecondaryColor = joinedItem.product.SecondaryColor,
                    Description = joinedItem.product.Description,
                    Category = joinedItem.product.Category,
                    Rating = joinedItem.top_20_product.combined_score
                })
                .OrderByDescending(joinedItem => joinedItem.Rating);

            var totalItems = query.Count();

            var products = query
                .Take(pageSize)
                .ToList(); // Materialize the query to execute it

            var setup = new Top20ListViewModel
            {
                Top20ViewModels = products,
                PaginationInfo = new PaginationInfo
                {
                    ItemsPerPage = pageSize,
                    TotalItems = totalItems
                },
                
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
        public IActionResult ProductDetails()
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

        public IActionResult ProductDisplay(int pageNum,  int pageSize, string? productType)
        {
            //pageSize = 10;

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
