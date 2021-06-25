using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentScheduling.Models
{
    // "IdentityDbContext" so we can use the tables provided by asp.net identity
    // and use the register and log in

    // <ApplicationUser> is there so we can add our new column from
    // ApplicationUser model to the default users table (AspNetUsers)
    // provided by Identity
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Appointment> Appointments { get; set; }
    }
}
