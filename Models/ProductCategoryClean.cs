using System.ComponentModel.DataAnnotations;

namespace Intex.Models
{
    public partial class ProductCategoryClean
    {
        [Key]
        public int p_id {  get; set; }
        
        public int c_id { get; set; }
    }
}
