using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Uplift.Models;

namespace Uplift.DataAccess.Data    // had to change the namespace since moved the Data folder to this class library
{                                   // from Uplift

    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // must add the models to access in our application
        public DbSet<Category> Category { get; set; }
        public DbSet<Frequency> Frequency { get; set; }

        public DbSet<Service> Service { get; set; }

        public DbSet<OrderHeader> OrderHeader{ get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }

        public DbSet<WebImages> WebImages { get; set; }
    }
}
