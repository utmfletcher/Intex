
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
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Globalization;



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
        private readonly string _onnxModelPath;
        private readonly InferenceSession _session;
        private Customer _customer;

        // Constructor uses dependency injection to populate the services
        public HomeController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IProductRepository repo)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _repo = repo;

            _onnxModelPath = Path.Combine(Path.GetFullPath(Environment.CurrentDirectory), "fraud_order_detector.onnx");
            _session = new InferenceSession(_onnxModelPath);
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
        [Authorize]
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
        [Authorize]
        public IActionResult PlaceOrder(string street, string city, string state, string country, string bank, string typeOfCard)
        {

            var cart = GetCurrentCart();
            var lineItems = _repo.Lineitems;
            var products = _repo.Products;
            var customers = _repo.Customers;
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
            var class_type_dict = new Dictionary<int, string>
            {
                { 0, "Not Fraud" },
                { 1, "Fraud" }
            };

            MinMaxScaler timeScaler = new MinMaxScaler(-2.83, 24);
            MinMaxScaler amountScaler = new MinMaxScaler(5, 400);
            MinMaxScaler totalValueScaler = new MinMaxScaler(1, 390);
            MinMaxScaler ageScaler = new MinMaxScaler(16.8, 76.9);
            MinMaxScaler dayOfMonth = new MinMaxScaler(0, 31);
            MinMaxScaler monthOfYear = new MinMaxScaler(1, 12);
            var input = new List<float>
            {
                (float)timeScaler.Scale((double)order.Time!),
                (float)amountScaler.Scale((double)order.Amount!),
                (float)totalValueScaler.Scale(CalculateTotalValue(order.TransactionId, lineItems, products)),
                //(float)ageScaler.Scale((double)customers.Where(c => c.CustomerId == order.CustomerId).Select(c => c.Age).SingleOrDefault()),
                (float)ageScaler.Scale(46.85),
                (float)dayOfMonth.Scale(DateTime.Parse(order.Date!).Day),
                (float)monthOfYear.Scale(DateTime.Parse(order.Date!).Month),
                order.DayOfWeek == "Monday" ? (float)1.0 : (float)0.0,
                order.DayOfWeek == "Saturday" ? (float)1.0 : (float)0.0,
                order.DayOfWeek == "Sunday" ? (float)1.0 : (float)0.0,
                order.DayOfWeek == "Thursday" ? (float)1.0 : (float)0.0,
                order.DayOfWeek == "Tuesday" ? (float)1.0 : (float)0.0,
                order.DayOfWeek == "Wednesday" ? (float)1.0 : (float)0.0,
                order.EntryMode == "PIN" ? (float)1.0 : (float)0.0,
                order.EntryMode == "Tap" ? (float)1.0 : (float)0.0,
                order.TypeOfTransaction == "Online" ? (float)1.0 : (float)0.0,
                order.TypeOfTransaction == "POS" ? (float)1.0 : (float)0.0,
                order.CountryOfTransaction == "India" ? (float)1.0 : (float)0.0,
                order.CountryOfTransaction == "Russia" ? (float)1.0 : (float)0.0,
                order.CountryOfTransaction == "USA" ? (float)1.0 : (float)0.0,
                order.CountryOfTransaction == "United Kingdom" ? (float)1.0 : (float)0.0,
                order.ShippingAddress == "India" ? (float)1.0 : (float)0.0,
                order.ShippingAddress == "Russia" ? (float)1.0 : (float)0.0,
                order.ShippingAddress == "USA" ? (float)1.0 : (float)0.0,
                order.ShippingAddress == "United Kingdom" ? (float)1.0 : (float)0.0,
                order.Bank == "HSBC" ? (float)1.0 : (float)0.0,
                order.Bank == "Halifax" ? (float)1.0 : (float)0.0,
                order.Bank == "Lloyds" ? (float)1.0 : (float)0.0,
                order.Bank == "Metro" ? (float)1.0 : (float)0.0,
                order.Bank == "Monzo" ? (float)1.0 : (float)0.0,
                order.Bank == "RBS" ? (float)1.0 : (float)0.0,
                order.TypeOfCard == "Visa" ? (float)1.0 : (float)0.0,
                customers.Where(c => c.CustomerId == order.CustomerId).Select(c => c.CountryOfResidence).SingleOrDefault()! == "Russia" ? (float)1.0 : (float)0.0,
                customers.Where(c => c.CustomerId == order.CustomerId).Select(c => c.CountryOfResidence).SingleOrDefault()! == "United Kingdom" ? (float)1.0 : (float)0.0,
                customers.Where(c => c.CustomerId == order.CustomerId).Select(c => c.Gender).SingleOrDefault()! == "M" ? (float)1.0 : (float)0.0,
                (float)1
            };
            var inputTensor = new DenseTensor<float>(input.ToArray(), new[] { 1, input.Count });

            var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
                };

            string predictionResult;
            using (var results = _session.Run(inputs))
            {
                var prediction = results.FirstOrDefault(item => item.Name == "output_label")?.AsTensor<long>().ToArray();
                predictionResult = prediction != null && prediction.Length > 0 ? class_type_dict.GetValueOrDefault((int)prediction[0], "Unknown") : "Error in prediction";
            }

            OrderPrediction result = (new OrderPrediction { Orders = order, Prediction = predictionResult });

            if (result.Prediction == "Not Fraud")
            {
                return RedirectToAction("OrderConfirmation");
            } else
            {
                return RedirectToAction("ReviewOrder");
            }
            // Optionally, you can remove the cart from the session here
            // HttpContext.Session.Remove("Cart");

            // Redirect to the confirmation page
            
        }


        // tabel.Orders. Add(Order)    another save changes 
        //HttpContext.Session.Remove(CartSession)
        //return View(page, order)
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AboutUs()
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

        public IActionResult ReviewOrder()
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
        public IActionResult ProductDashboard()
        {
            // Retrieve all CleanProducts using LINQ, ordered by product_id in ascending order.
            var cleanProductsList = _repo.CleanProducts.OrderBy(p => p.product_id).ToList();

            // Pass the sorted list of products to the view Keep this one
            return View(cleanProductsList);
        }

        //[Authorize(Roles = "Admin")]

        //public IActionResult OrderDashboard()
        //{
        //    // Retrieve all CleanProducts using LINQ, ordered by product_id in ascending order.
        //    var Orders = _repo.Orders .OrderBy(p => p.Date).ToList();
        
        //    // Pass the sorted list of products to the view Keep this one
        //    return View(Orders);
        //}



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

        [Authorize(Roles = "Admin")]

        public IActionResult OrderDashboard()
        {
            var products = _repo.Products;
            var lineItems = _repo.Lineitems;
            var size = _repo.Orders.ToList().Count;
            //var orders = _repo.Orders.OrderBy(o => o.Date).Take(20).ToList();
            var today = DateTime.Now;
            var sortedOrders = _repo.Orders
            .AsEnumerable()
            .Select(order => new {
                Order = order,
                ParsedDate = ParseDate(order.Date)
            })
            .Where(x => x.ParsedDate != null && x.ParsedDate <= today)
            .OrderByDescending(x => x.ParsedDate)
            .ThenByDescending(x => x.Order.Time)
            .Select(x => x.Order)
            .Take(20)
            .ToList();
            var customers = _repo.Customers;
            var predictions = new List<OrderPrediction>();
            var class_type_dict = new Dictionary<int, string>
            {
                { 0, "Not Fraud" },
                { 1, "Fraud" }
            };

            MinMaxScaler timeScaler = new MinMaxScaler(-2.83, 24);
            MinMaxScaler amountScaler = new MinMaxScaler(5, 400);
            MinMaxScaler totalValueScaler = new MinMaxScaler(1, 390);
            MinMaxScaler ageScaler = new MinMaxScaler(16.8, 76.9);
            MinMaxScaler dayOfMonth = new MinMaxScaler(0, 31);
            MinMaxScaler monthOfYear = new MinMaxScaler(1, 12);
            foreach (var order in sortedOrders)
            {
                var input = new List<float>
                {
                    (float)timeScaler.Scale((double)order.Time!),
                    (float)amountScaler.Scale((double)order.Amount!),
                    (float)totalValueScaler.Scale(CalculateTotalValue(order.TransactionId, lineItems, products)),
                    //(float)ageScaler.Scale((double)customers.Where(c => c.CustomerId == order.CustomerId).Select(c => c.Age).SingleOrDefault()),
                    (float)ageScaler.Scale(46.85),
                    (float)dayOfMonth.Scale(DateTime.Parse(order.Date!).Day),
                    (float)monthOfYear.Scale(DateTime.Parse(order.Date!).Month),
                    order.DayOfWeek == "Monday" ? (float)1.0 : (float)0.0,
                    order.DayOfWeek == "Saturday" ? (float)1.0 : (float)0.0,
                    order.DayOfWeek == "Sunday" ? (float)1.0 : (float)0.0,
                    order.DayOfWeek == "Thursday" ? (float)1.0 : (float)0.0,
                    order.DayOfWeek == "Tuesday" ? (float)1.0 : (float)0.0,
                    order.DayOfWeek == "Wednesday" ? (float)1.0 : (float)0.0,
                    order.EntryMode == "PIN" ? (float)1.0 : (float)0.0,
                    order.EntryMode == "Tap" ? (float)1.0 : (float)0.0,
                    order.TypeOfTransaction == "Online" ? (float)1.0 : (float)0.0,
                    order.TypeOfTransaction == "POS" ? (float)1.0 : (float)0.0,
                    order.CountryOfTransaction == "India" ? (float)1.0 : (float)0.0,
                    order.CountryOfTransaction == "Russia" ? (float)1.0 : (float)0.0,
                    order.CountryOfTransaction == "USA" ? (float)1.0 : (float)0.0,
                    order.CountryOfTransaction == "United Kingdom" ? (float)1.0 : (float)0.0,
                    order.ShippingAddress == "India" ? (float)1.0 : (float)0.0,
                    order.ShippingAddress == "Russia" ? (float)1.0 : (float)0.0,
                    order.ShippingAddress == "USA" ? (float)1.0 : (float)0.0,
                    order.ShippingAddress == "United Kingdom" ? (float)1.0 : (float)0.0,
                    order.Bank == "HSBC" ? (float)1.0 : (float)0.0,
                    order.Bank == "Halifax" ? (float)1.0 : (float)0.0,
                    order.Bank == "Lloyds" ? (float)1.0 : (float)0.0,
                    order.Bank == "Metro" ? (float)1.0 : (float)0.0,
                    order.Bank == "Monzo" ? (float)1.0 : (float)0.0,
                    order.Bank == "RBS" ? (float)1.0 : (float)0.0,
                    order.TypeOfCard == "Visa" ? (float)1.0 : (float)0.0,
                    customers.Where(c => c.CustomerId == order.CustomerId).Select(c => c.CountryOfResidence).SingleOrDefault()! == "Russia" ? (float)1.0 : (float)0.0,
                    customers.Where(c => c.CustomerId == order.CustomerId).Select(c => c.CountryOfResidence).SingleOrDefault()! == "United Kingdom" ? (float)1.0 : (float)0.0,
                    customers.Where(c => c.CustomerId == order.CustomerId).Select(c => c.Gender).SingleOrDefault()! == "M" ? (float)1.0 : (float)0.0,
                    (float)1
                };
                var inputTensor = new DenseTensor<float>(input.ToArray(), new[] { 1, input.Count });

                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
                };

                string predictionResult;
                using (var results = _session.Run(inputs))
                {
                    var prediction = results.FirstOrDefault(item => item.Name == "output_label")?.AsTensor<long>().ToArray();
                    predictionResult = prediction != null && prediction.Length > 0 ? class_type_dict.GetValueOrDefault((int)prediction[0], "Unknown") : "Error in prediction";
                }

                predictions.Add(new OrderPrediction { Orders = order, Prediction = predictionResult });
            }

            return View(predictions);

        }

        public static long CalculateTotalValue(int transactionId, IQueryable<Lineitem> lineItems, IQueryable<Product> products)
        {
            long totalValue = (long)lineItems
                .Where(l => l.TransactionId == transactionId)
                .Join(products,
                      lineItem => lineItem.ProductId,
                      product => product.ProductId,
                      (lineItem, product) => new { lineItem.Qty, product.Price })
                .Sum(x => x.Price * x.Qty)!;

            return totalValue;
        }

        private static DateTime? ParseDate(string dateString)
        {
            DateTime dateValue;
            string[] formats = { "MM/dd/yyyy", "M/d/yyyy", "MM/d/yyyy", "M/dd/yyyy" };
            if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
            {
                return dateValue;
            }
            return null;
        }


        public IActionResult AdminDashboard()
        {
            return View();
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult EditProduct(int id)
        {
            var product = _repo.CleanProducts.FirstOrDefault(p => p.product_id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult EditProduct(CleanProduct product)
        {
            if (ModelState.IsValid)
            {
                _repo.UpdateCleanProduct(product);
                _repo.SaveChanges();

                return RedirectToAction(nameof(ProductDashboard));
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

            return RedirectToAction(nameof(ProductDashboard));
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateProduct()
        {
            var viewModel = new CleanProductViewModel();
            return View(viewModel); // Ensure you're passing CleanProductViewModel instance to the view
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateProduct(CleanProduct response)
        {
            // Check if the model state is valid (i.e., if there are no validation errors)

            // Add the product to the repository
            _repo.AddCleanProduct(response);

            _repo.SaveChanges();

            // Redirect to the "OrderConfirmation" action if product creation is successful
            return RedirectToAction("ProductDashboard");



        }



        /*[HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            // Implement admin dashboard logic here
            return View();
        }*/

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
