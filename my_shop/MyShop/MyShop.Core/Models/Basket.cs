using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Core.Models
{
    public class Basket : BaseEntity
    {
        // has all the properties from BaseEntity as well
        
        public virtual ICollection<BasketItem> BasketItems { get; set; }

        public Basket() {
            this.BasketItems = new List<BasketItem>();
        }
    }
}
