using System.Security.Principal;
using Microsoft.AspNetCore.Identity;

namespace UserManagementApi.Model
{
    public class Users : IdentityUser 
    {
         public string? FirstName { get; set; }
        public string? LastName { get; set; }
         public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
