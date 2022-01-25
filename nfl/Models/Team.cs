﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NFL.Models
{
  public class Team
  {
    public string TeamID { get; set; }
    public string Name { get; set; }
    public Conference Conference { get; set; }
    public Division Division { get; set; }
    public string LogoImage { get; set; }
  }
}
