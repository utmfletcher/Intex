using Microsoft.Build.Evaluation;

namespace Intex.Models
{
    public class Cart
    {
        //we can  store list of lines. each line has  cartLine class information
        public List<Cartline> Lines { get; set; } = new List<Cartline>();
        public virtual void AddItem(Product prod, int quantity) // when we choose to add we pass quantitiy and product
        {
            //pass product that contains product id and then get first or default
            Cartline? line = Lines  // build instance  and find in array where it equals product id 
                .Where(x => x.Product.ProductId == prod.ProductId)
                .FirstOrDefault();
            //if product not found in cart
            //was item already added to cart?
            if(line == null)
            {
                Lines.Add(new Cartline
                {
                    Product = prod,
                    Quantity = quantity
                   
                });

            }
            else
            {
                //update quanitity
                line.Quantity += quantity;  
            }
        }
        //remove product where produc id is equal to passed in product
        public virtual void RemoveLine(Product prod)=>Lines.RemoveAll(x=>x.Product.ProductId == prod.ProductId);
       //add ability to clear cart
        public virtual void Clear()=> Lines.Clear();
        //PRICE CALCULATOR 
        public int? CalculateTotal() => Lines.Sum(line => line.Quantity * line.Product.Price);
        public class Cartline
        {
            public int CartLineId { get; set; }
            public Product Product { get; set; }
            public int Quantity { get; set; }
        }

    }
}
