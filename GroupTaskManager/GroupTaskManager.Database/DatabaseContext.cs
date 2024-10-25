using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace GroupTaskManager.GroupTaskManager.Database
{
    public class DatabaseContext : IdentityDbContext<UserModel>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
