using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace Intex.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

    

    }
}
