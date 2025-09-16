using API.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace API.DataAccessLayer
{
    public class APIDbContext : DbContext
    {
        // Constructor accepting DbContextOptions
        public APIDbContext(DbContextOptions<APIDbContext> options) : base(options)
        {
        }

        // DbSet for each entity/table
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }

        // Optional: Override OnModelCreating if needed for custom configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Additional configuration if needed
        }
    }
}
