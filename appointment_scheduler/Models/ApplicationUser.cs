using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentScheduling.Models
{
    // create this model bc we wanted to add another column
    // to the user table that we got by default with Identity

    // ApplicationUser will get all of the properties of
    // IdentityUser and whatever we add
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
