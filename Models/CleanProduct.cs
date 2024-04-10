using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Intex.Models;

public partial class CleanProduct
{
    [Key]
    public int product_id { get; set; }

    public string? name { get; set; }

    public int? year { get; set; }

    public int? num_parts { get; set; }

    public int? price { get; set; }

    public string? img_link { get; set; }

    public string? primary_color { get; set; }

    public string? secondary_color { get; set; }

    public string? description { get; set; }

}
