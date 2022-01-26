using Microsoft.AspNetCore.Identity;
using PavolsBookStore.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PavolsBookStore.Areas.Admin.Models
{
  public class UserViewModel
  {
    public IEnumerable<User> Users { get; set; }
    public IEnumerable<IdentityRole> Roles { get; set; }
  }
}
