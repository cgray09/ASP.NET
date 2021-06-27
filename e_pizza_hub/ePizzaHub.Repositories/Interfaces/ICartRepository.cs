using ePizzaHub.Entities;
using ePizzaHub.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ePizzaHub.Repositories.Interfaces
{
    // by ": IRepository<Cart>" all methods in IRepository are now
    // part of this repo
    public interface ICartRepository : IRepository<Cart>
    {
        Cart GetCart(Guid CartId);
        CartModel GetCartDetails(Guid CartId);
        int DeleteItem(Guid cartId, int itemId);
        int UpdateQuantity(Guid cartId, int itemId, int Quantity);
        int UpdateCart(Guid cartId, int userId);
    }
}
