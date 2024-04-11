using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Intex.Models;

public partial class ItemReccomendation
{
    [Key]
    public int product_ID { get; set; }

    public string name { get; set; }

    //product 1 
    public int? Recommendation_ID_1 { get; set; }
    public string? Recommendation_Name_1 { get; set; }
    public string? Recommendation_ImgLink_1 { get; set; }
    public int? Recommendation_Price_1 { get; set; }

    //product 2 
    public int? Recommendation_ID_2 { get; set; }
    public string? Recommendation_Name_2 { get; set; }
    public string? Recommendation_ImgLink_2 { get; set; }
    public int? Recommendation_Price_2 { get; set; }


    //product 3 
    public int? Recommendation_ID_3 { get; set; }
    public string? Recommendation_Name_3 { get; set; }
    public string? Recommendation_ImgLink_3 { get; set; }
    public int? Recommendation_Price_3 { get; set; }


    //product 4 
    public int? Recommendation_ID_4 { get; set; }
    public string? Recommendation_Name_4 { get; set; }
    public string? Recommendation_ImgLink_4 { get; set; }
    public int? Recommendation_Price_4 { get; set; }


    //product 5 
    public int? Recommendation_ID_5 { get; set; }
    public string? Recommendation_Name_5 { get; set; }
    public string? Recommendation_ImgLink_5 { get; set; }
    public int? Recommendation_Price_5 { get; set; }

  




}
