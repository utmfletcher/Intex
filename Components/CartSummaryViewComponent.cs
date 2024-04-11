using Microsoft.AspNetCore.Mvc;
using Intex.Models;

namespace Intex.Components
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private Cart cart;
        public CartSummaryViewComponent(Cart cartService)
        {
            cart= cartService;
        }
        public IViewComponentResult Invoke()
        {
            return View(cart);
        }


    }
}
