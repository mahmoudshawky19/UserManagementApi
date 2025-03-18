using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserManagementApi.Model;

namespace Data_Access
{
    public class ApplicationDbContext : IdentityDbContext <Users>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options)
        {
            
        }
        public DbSet<Users> users { get; set; }
    }
}
