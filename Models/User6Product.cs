using System.ComponentModel.DataAnnotations;

namespace Intex.Models
{
    public class User6Product
    {
        [Key]
        public int product_ID { get; set; }

        public string RecommendedLEGOName { get; set; }

        public string index { get;set; }
    }
}
