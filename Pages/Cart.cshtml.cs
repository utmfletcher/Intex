using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Intex.Models;
using Intex.Infastructure;

namespace Intex.Pages
{
    public class CartModel : PageModel
    {
        private IProductRepository _repo;
        public Cart Cart { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public CartModel(IProductRepository temp,Cart cartService)
        {
            _repo= temp;    
            Cart=cartService;

        }

        
        //= new Cart();
        public string ReturnUrl { get; set; } = "/";

        public void OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl ?? "/";
            //Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
        }



        public IActionResult OnPost(int productId, string returnUrl)
        {
            Product prod = _repo.Products.FirstOrDefault(x => x.ProductId == productId);
            if (prod != null)
            {
                Cart.AddItem(prod, 1);
            }

            // Redirect to the specified returnUrl or default to "/"
            return RedirectToPage(new { returnUrl = returnUrl ?? "/" });
        }
        public IActionResult OnPostRemove(int productId,string returnUrl)
        {
            Cart.RemoveLine(Cart.Lines.First(x => x.Product.ProductId == productId).Product);
            return RedirectToPage(new {returnUrl = returnUrl ?? "/"});
        }
      

    }
}
