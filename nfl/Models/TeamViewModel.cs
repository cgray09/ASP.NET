using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NFL.Models
{
  public class TeamViewModel
  {
    public Team Team { get; set; }
    public string ActiveConf { get; set; } = "all";
    public string ActiveDiv { get; set; } = "all";
  }
}
