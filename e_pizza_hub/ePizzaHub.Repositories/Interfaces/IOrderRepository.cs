﻿using ePizzaHub.Entities;
using ePizzaHub.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ePizzaHub.Repositories.Interfaces
{
    // ": IRepository<Order>" is needed so we can do the basic operations
    // from IRepository in IOrderRepository 
    public interface IOrderRepository: IRepository<Order>
    {
        IEnumerable<Order> GetUserOrders(int UserId);
        OrderModel GetOrderDetails(string id);
        PagingListModel<OrderModel> GetOrderList(int page, int pageSize);
    }
}
