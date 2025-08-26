using GreatState.DotnetTest.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GreatState.DotnetTest.API
{


    public class CMSPages : DbContext
    {
        public CMSPages(DbContextOptions<CMSPages> options)
            : base(options) { }

        public DbSet<Page> Pages => Set<Page>();
        public DbSet<Role> Roles => Set<Role>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Page>()
                .HasData(
                new Page { Id = 1, Title = "Public page", Body = "This page contains info about our products", RoleRequired = "anonymous" },
                new Page { Id = 2, Title = "Staff page", Body = "This page is for staff only and should be protected", RoleRequired = "staff" },
                new Page { Id = 3, Title = "Admin page", Body = "This page is for admin only and should be protected", RoleRequired = "admin" }
                );
            modelBuilder.Entity<Role>()
               .HasData(
               new Role { Id = 1, Key = "1234-anon", Name = "anonymous" },
               new Role { Id = 2,  Key = "1234-staff", Name = "staff" },
               new Role { Id = 3,  Key = "1234-admin", Name = "admin" }
               );
        }
    }
}
