using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Uplift.Models
{
    public class Category
    {
        [Key]   // "key" makes it a primary key and auto-increment
        public int Id { get; set; }

        [Required]
        [Display(Name="Category Name")] // so w/o display it will just display name
        public string Name { get; set; }

        [Required]
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }
    }
}
