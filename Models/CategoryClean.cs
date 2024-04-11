using System.ComponentModel.DataAnnotations;

namespace Intex.Models
{
    public partial class CategoryClean
    {
        [Key]
        public int category_id { get; set; }
        public string? name { get; set;}
    }
}
