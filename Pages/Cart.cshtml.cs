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

        public Cart Cart { get; set; }= new Cart();

        public void OnGet()
        {
            Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
        }
        public void OnPost(int productId)
        {
            Product prod =_repo.Products
                .FirstOrDefault(x => x.ProductId == productId);
            if(prod != null ) 
            {
                Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
                Cart.AddItem(prod, 1);
                HttpContext.Session.SetJson("cart", Cart);


            }
        }
    }
}
