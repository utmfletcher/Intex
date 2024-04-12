using System.ComponentModel.DataAnnotations;
namespace Intex.Models
{
    public class UserRoles
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public List<string> Roles { get; set; }
    }
}