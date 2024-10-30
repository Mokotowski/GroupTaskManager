using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace GroupTaskManager.GroupTaskManager.Database
{
    public class DatabaseContext : IdentityDbContext<UserModel>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<UserModel> UserModel {  get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<Group_User> Group_User { get; set; }
        public DbSet<TaskRecord> TaskRecord { get; set; }
        public DbSet<TaskAnswer> TaskAnswer { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Group>()
                .HasOne(g => g.User)
                .WithMany(u => u.Group)
                .HasForeignKey(f => f.Id_User);

            modelBuilder.Entity<Group_User>()
                .HasOne(g => g.Group)
                .WithMany(u => u.Group_User)
                .HasForeignKey(f => f.Id_Group)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskRecord>()
                .HasOne(g => g.Group)
                .WithMany(u => u.TaskRecord)
                .HasForeignKey(f => f.Id_Group)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskAnswer>()
                .HasOne(g => g.TaskRecord)
                .WithMany(u => u.TaskAnswer)
                .HasForeignKey(f => f.Id_Task);


            base.OnModelCreating(modelBuilder);
        }
    }
}
