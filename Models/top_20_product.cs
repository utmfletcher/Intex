using System.ComponentModel.DataAnnotations;

namespace Intex.Models
{
    public class top_20_product
    {
        [Key]
        public int product_ID { get; set; }

        public double rating { get; set; }  

        public int qty { get; set; }
        public double combined_score { get; set;}
    }
}
