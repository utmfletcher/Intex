using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Intex.Models;
using Intex.Infastructure;

namespace Intex.Pages
{
    public class CartModel : PageModel
    {
        private IProductRepository _repo;
        public CartModel(IProductRepository temp)
        {
            _repo= temp;    
        }

        public Cart Cart { get; set; }
        //= new Cart();
        public string ReturnUrl { get; set; } = "/";

        public void OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl ?? "/";
            Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
        }
        public IActionResult OnPost(int productId,string returnUrl)
        {
            Product prod =_repo.Products
                .FirstOrDefault(x => x.ProductId == productId);
            if(prod != null ) 
            {
                Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
                Cart.AddItem(prod, 1);
                HttpContext.Session.SetJson("cart", Cart);
            }
            //redirect to oage and pass return url so its set in there
            return RedirectToPage(new { ReturnUrl = returnUrl });
        }
    }
}
