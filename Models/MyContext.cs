using Microsoft.EntityFrameworkCore;
using System;
namespace Rntr.Models
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Tool> Tools { get; set; }
        public DbSet<Rental> Rentals { get; set; }

    }
}