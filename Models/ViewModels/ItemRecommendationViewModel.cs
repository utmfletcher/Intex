using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Intex.Models.ViewModels;

public partial class ItemRecommendationViewModel
{
    [Key]
    public int? RecommendationId { get; set; }

    public string? Name { get; set; }

    public int? Price { get; set; }

    public string? ImgLink { get; set; }

}
