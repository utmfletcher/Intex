
using Intex.Infastructure;
using Intex.Models;
using Intex.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Construction;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System.Diagnostics;
using System.Drawing.Printing;


namespace Intex.Controllers
{
    public class HomeController : Controller
    {
       // private readonly ILogger<HomeController> _logger;

        private IProductRepository _repo;
        private const string Key = "Cart";

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
        public  IActionResult Cart()
        {
            var cart = GetCurrentCart();
            if(cart.Lines.Count == 0)
            {
                return View("EmptyCart");

            }
            return View(cart);
        }
        private Cart GetCurrentCart()
        {
            var cart = HttpContext.Session.GetJson<Cart>(Key);
           // return HttpContext.Session.GetJson<SessionCart>(CartSessionKey) ?? new SessionCart();
            if(cart == null)
            {
                cart = new Cart();
                HttpContext.Session.SetJson(Key, cart);
            }
            return cart;
        }
        public IActionResult Checkout()
        {
            var cart = GetCurrentCart();
            if (cart.Lines.Count == 0)
            {
                return View("EmptyCart");

            }
            return View(cart);
        }
        [HttpGet]
        /*   public async Task<IActionResult> Checkout()
           {
               if(!User.Identity.IsAuthenticated)
               {
                  // string checkoutUrl = Url.Action("Checkout", controller: Customer);

               }
           }*/
        public IActionResult PlaceOrder(string street, string city, string state, string country, string bank, string typeOfCard)
        {
            var cart = GetCurrentCart();
            var order = new Order
            {
                TransactionId = new Random().Next(100000, 999999),
                CustomerId = new Random().Next(10000, 999999),
                Date = DateOnly.FromDateTime(DateTime.Now).ToString(),
                DayOfWeek = DateTime.Now.DayOfWeek.ToString(),
                Time = (byte)DateTime.Now.Hour,
                EntryMode = "CVC",
                Amount = (short?)cart.CalculateTotal(),
                TypeOfTransaction = "Online",
                CountryOfTransaction = country,
                ShippingAddress = $"{street},{city},{state}",
                Bank = bank,
                TypeOfCard = typeOfCard,
                Fraud = 0 // still need determination
            };

             // Add the order to the context
            //_repo.SaveChanges(); // Save changes to the database

            // Optionally, you can remove the cart from the session here
            // HttpContext.Session.Remove("Cart");

            // Redirect to a confirmation page or another action method
            return RedirectToAction("OrderConfirmation");
        }

        // tabel.Orders. Add(Order)    another save changes 
        //HttpContext.Session.Remove(CartSession)
        //return View(page, order)
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


        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            // Retrieve all CleanProducts using LINQ.
            var cleanProductsList = _repo.CleanProducts.ToList();

            // Pass the list of products to the view.
            return View(cleanProductsList);
        }

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
