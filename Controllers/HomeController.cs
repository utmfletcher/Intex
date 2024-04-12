
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
using Microsoft.AspNetCore.Identity;


namespace Intex.Controllers
{

    public class HomeController : Controller
    {
        // Services are declared here
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
     

       // private readonly ILogger<HomeController> _logger;

        private IProductRepository _repo;
        private const string Key = "Cart";


        // Constructor uses dependency injection to populate the services
        public HomeController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IProductRepository repo)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _repo = repo;
        }

        public IActionResult Index()
        {
            int pageSize = 20;

            // User 6 Query 
            var UserQuery = _repo.Products
                .Join(_repo.User6Products,
                      product => product.ProductId,
                      User6Product => User6Product.product_ID,
                      (product, User6Product) => new { product, User6Product })
                .Select(joinedItem => new User6ViewModel
                {
                    ProductId = joinedItem.product.ProductId,
                    Name = joinedItem.User6Product.RecommendedLEGOName,
                    Year = joinedItem.product.Year,
                    NumParts = joinedItem.product.NumParts,
                    Price = joinedItem.product.Price,
                    ImgLink = joinedItem.product.ImgLink,
                    PrimaryColor = joinedItem.product.PrimaryColor,
                    SecondaryColor = joinedItem.product.SecondaryColor,
                    Description = joinedItem.product.Description,
                    Category = joinedItem.product.Category
                });

            var userProducts = UserQuery
                .Take(pageSize)
                .ToList(); // Materialize the query to execute it

            // Top 20 Query 
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


            var products = query
                .Take(pageSize)
                .ToList(); // Materialize the query to execute it

            var setup = new Top20ListViewModel
            {
                Top20ViewModels = products,
                User6ViewModels = userProducts

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

        /*   public async Task<IActionResult> Checkout()
           {
               if(!User.Identity.IsAuthenticated)
               {
                  // string checkoutUrl = Url.Action("Checkout", controller: Customer);

               }
           }*/
        [HttpPost]
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

            _repo.AddOrder(order); // Add the order to the repository
            _repo.SaveChanges();

            // Optionally, you can remove the cart from the session here
            // HttpContext.Session.Remove("Cart");

            // Redirect to the confirmation page
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
        public IActionResult OrderConfirmation()
        {
            return View();

        }



        public IActionResult ProductDetails(int productId)
        {
            

            var selectedProduct = _repo.CleanProducts
                .Where(p => p.product_id == productId)
                .Join(_repo.ProductCategories,
                      product => product.product_id,
                      productCategory => productCategory.p_id,
                      (product, productCategory) => new { product, productCategory })
                .Join(_repo.Categories,
                      combined => combined.productCategory.c_id,
                      category => category.category_id,
                      (combined, category) => new { combined.product, CategoryName = category.name })
                .Select(combined => new CleanProductViewModel
                {
                    ProductId = combined.product.product_id,
                    Name = combined.product.name,
                    Year = combined.product.year,
                    NumParts = combined.product.num_parts,
                    Price = combined.product.price,
                    ImgLink = combined.product.img_link,
                    PrimaryColor = combined.product.primary_color,
                    SecondaryColor = combined.product.secondary_color,
                    Description = combined.product.description,
                    CategoryNames = new List<string> { combined.CategoryName }
                })
                .FirstOrDefault(); // Get the first matching product or null if not found

            // second query 
            int pageSize = 5;


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

            // third Query 
            // Fetch the item recommendations from the database and then process them in-memory
            var recommendationData = _repo.ItemReccomendations
                .Where(pr => pr.product_ID == productId)
                .AsEnumerable() // Switch to LINQ to Objects for in-memory processing
                .SelectMany(pr => new List<ItemRecommendationViewModel> {
            new ItemRecommendationViewModel {
                RecommendationId = pr.Recommendation_ID_1,
                Name = pr.Recommendation_Name_1,
                ImgLink = pr.Recommendation_ImgLink_1,
                Price = pr.Recommendation_Price_1
            },
            new ItemRecommendationViewModel {
                RecommendationId = pr.Recommendation_ID_2,
                Name = pr.Recommendation_Name_2,
                ImgLink = pr.Recommendation_ImgLink_2,
                Price = pr.Recommendation_Price_2
            },
            new ItemRecommendationViewModel {
                RecommendationId = pr.Recommendation_ID_3,
                Name = pr.Recommendation_Name_3,
                ImgLink = pr.Recommendation_ImgLink_3,
                Price = pr.Recommendation_Price_3
            },
            new ItemRecommendationViewModel {
                RecommendationId = pr.Recommendation_ID_4,
                Name = pr.Recommendation_Name_4,
                ImgLink = pr.Recommendation_ImgLink_4,
                Price = pr.Recommendation_Price_4
            },
            new ItemRecommendationViewModel {
                RecommendationId = pr.Recommendation_ID_5,
                Name = pr.Recommendation_Name_5,
                ImgLink = pr.Recommendation_ImgLink_5,
                Price = pr.Recommendation_Price_5
            }
            // Add other recommendations as necessary
                    })
                    .ToList();


            var setup = new ProductListViewModel
            {
                Top20ViewModels = products,
                CleanProducts = new List<CleanProductViewModel> { selectedProduct },
                img_link = selectedProduct.ImgLink,
                ItemReccomendations = recommendationData
                // Initialize other necessary properties of ProductListViewModel if there are any
            };

            return View(setup);
        }




        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            // Retrieve all CleanProducts using LINQ, ordered by product_id in ascending order.
            var cleanProductsList = _repo.CleanProducts.OrderBy(p => p.product_id).ToList();

            // Pass the sorted list of products to the view Keep this one
            return View(cleanProductsList);
        }


        public IActionResult ProductDisplay(int pageNum,  int pageSize, string? productType, string? productColour)
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
            if (!string.IsNullOrWhiteSpace(productColour))
            {
                query = query.Where(p => p.product.primary_color == productColour || p.product.secondary_color == productColour);
            }

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




        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> ListUsers()
        //{
        //    var users = _userManager.Users.ToList();
        //    var userRolesViewModel = new List<UserRolesViewModel>();

        //    foreach (var user in users)
        //    {
        //        var thisViewModel = new UserRolesViewModel
        //        {
        //            UserId = user.Id,
        //            UserName = user.UserName,
        //            Email = user.Email,
        //            Roles = await _userManager.GetRolesAsync(user)
        //        };
        //        userRolesViewModel.Add(thisViewModel);
        //    }

        //    return View(userRolesViewModel);
        //}




















        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult EditProduct(CleanProduct product)
        {
            if (ModelState.IsValid)
            {
                _repo.UpdateCleanProduct(product);
                _repo.SaveChanges();
                return RedirectToAction(nameof(AdminDashboard));
            }
            return View(product);
        }
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> ListUsers()
        //{
        //    var users = _userManager.Users.Select(user => new UserRolesViewModel
        //    {
        //        UserId = user.Id,
        //        UserName = user.UserName,
        //        Email = user.Email,
        //        Roles = _userManager.GetRolesAsync(user).Result  // Synchronous call for simplicity, better to use async in real applications
        //    }).ToList();

        //    return View(users);
        //}


        // Displays the delete confirmation page
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _repo.CleanProducts.FirstOrDefault(p => p.product_id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View("DeleteConfirmation", product);
        }

        // Processes the deletion of a product
        [HttpPost, ActionName("DeleteProduct")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteProductConfirmed(int id)
        {
            var product = _repo.CleanProducts.FirstOrDefault(p => p.product_id == id);
            if (product != null)
            {
                _repo.DeleteCleanProduct(product); // Assume your repository has this method
                _repo.SaveChanges();
            }

            return RedirectToAction(nameof(AdminDashboard));
        }
        // Display the form for adding a new product
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateProduct()
        {
            return View("CreateProduct", new CleanProduct()); // Pass a new product to the view
        }

        // Process the form submission for a new product
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateProduct(CleanProduct product)
        {
            if (ModelState.IsValid)
            {
                _repo.AddCleanProduct(product); // Add the product to the database
                _repo.SaveChanges(); // Save the changes
                return RedirectToAction(nameof(AdminDashboard)); // Redirect to the dashboard
            }

            return View(product); // If invalid, show the form again with validation messages
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
