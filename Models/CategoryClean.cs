using System.ComponentModel.DataAnnotations;

namespace Intex.Models
{
    public partial class CategoryClean
    {
        //internal object categoryId;

        [Key]
        public int category_id { get; set; }
        public string? name { get; set;}
    }
}
