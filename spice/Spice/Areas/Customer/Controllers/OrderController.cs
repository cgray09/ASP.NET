using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;

/*
    when looking at controllers look at them on a higher level from the perspective
    of what the user will see in the view and everything that each method will need
    to do its job (like what we need to grab from the db)
*/

namespace Spice.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _db;
        private int PageSize = 2;
        public OrderController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }


        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = await _db.OrderHeader.Include(o => o.ApplicationUser).FirstOrDefaultAsync(o => o.Id == id && o.UserId == claim.Value),
                OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == id).ToListAsync()
            };

            return View(orderDetailsViewModel);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetOrderStatus(int Id)
        {
            return PartialView("_OrderStatus", _db.OrderHeader.Where(m => m.Id == Id).FirstOrDefault().Status);

        }

        // gets called originally from _Layout.cshtml since the link is in the header
        // so the moment the link is clicked the PageInfo model is created
        // for the custom tag helper by this function. (this action display the OrderHistory view)
        [Authorize]
        public async Task<IActionResult> OrderHistory(int productPage=1)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // needed for pagination
            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            

            List<OrderHeader> OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser).Where(u => u.UserId == claim.Value).ToListAsync();

            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                };
                orderListVM.Orders.Add(individual);
            }

            var count = orderListVM.Orders.Count;
            orderListVM.Orders = orderListVM.Orders.OrderByDescending(p => p.OrderHeader.Id)
                                 .Skip((productPage - 1) * PageSize) // skip neccesary pages to get to correct page where desired records are
                                 .Take(PageSize).ToList();

            orderListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = count,
                urlParam = "/Customer/Order/OrderHistory?productPage=:" // url to this action
            };

            // just by calling "View" it calls the view with the same name as the action
            return View(orderListVM);
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)] // admin protection
        public async Task<IActionResult> ManageOrder(int productPage = 1)
        {
           
            List<OrderDetailsViewModel> orderDetailsVM = new List<OrderDetailsViewModel>();

            List<OrderHeader> OrderHeaderList = await _db.OrderHeader.Where(o=>o.Status==SD.StatusSubmitted || o.Status==SD.StatusInProcess).OrderByDescending(u=>u.PickUpTime).ToListAsync();
            

            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                };
                orderDetailsVM.Add(individual);
            }

            
            // i think "OrderBy" may be built in since "orderDetailsVM" is a list
            return View(orderDetailsVM.OrderBy(o=>o.OrderHeader.PickUpTime).ToList());
        }


        public async Task<IActionResult> GetOrderDetails(int Id)
        {
            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = await _db.OrderHeader.Include(el => el.ApplicationUser).FirstOrDefaultAsync(m => m.Id == Id),
                OrderDetails = await _db.OrderDetails.Where(m => m.OrderId == Id).ToListAsync()
            };
            //orderDetailsViewModel.OrderHeader.ApplicationUser = await _db.ApplicationUser.FirstOrDefaultAsync(u => u.Id == orderDetailsViewModel.OrderHeader.UserId);

            // IActionResult return type also allows us to return a partial
            return PartialView("_IndividualOrderDetails", orderDetailsViewModel);
        }




        [Authorize(Roles =SD.KitchenUser + ","+ SD.ManagerUser)]
        public async Task<IActionResult> OrderPrepare(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusInProcess;
            await _db.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");
        }


        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderReady(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusReady;
            await _db.SaveChangesAsync();

            //Email logic to notify user that order is ready for pickup
            await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserId).FirstOrDefault().Email, "Spice - Order Ready for Pickup " + orderHeader.Id.ToString(), "Order is ready for pickup.");


            return RedirectToAction("ManageOrder", "Order");
        }


        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderCancel(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusCancelled;
            await _db.SaveChangesAsync();
            await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserId).FirstOrDefault().Email, "Spice - Order Cancelled " + orderHeader.Id.ToString(), "Order has been cancelled successfully.");

            return RedirectToAction("ManageOrder", "Order");
        }


        /*
            This controller in a nutshell just gets a user specified order and if not given then all orders and
            adds them to "orderListVM" for viewing on the view (long and tedious but simple)
        */

        [Authorize]
        public async Task<IActionResult> OrderPickup(int productPage = 1, string searchEmail=null, string searchPhone = null, string searchName = null)
        {
            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            // StringBuilder to build on the url
            StringBuilder param = new StringBuilder();
            param.Append("/Customer/Order/OrderPickup?productPage=:");
            
            param.Append("&searchName=");
            if(searchName!=null)
            {
                param.Append(searchName);
            }
            param.Append("&searchEmail=");
            if (searchEmail != null)
            {
                param.Append(searchEmail);
            }
            param.Append("&searchPhone=");
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }

            List<OrderHeader> OrderHeaderList = new List<OrderHeader>(); // initialiazed here so we can you in any of the conditions below
            if (searchName != null || searchEmail != null || searchPhone != null)
            {
                var user = new ApplicationUser();

                if(searchName!=null)
                {
                    OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser)
                                                .Where(u => u.PickupName.ToLower().Contains(searchName.ToLower()))
                                                .OrderByDescending(o => o.OrderDate).ToListAsync();
                }
                else
                {
                    if (searchEmail != null)
                    {
                        user = await _db.ApplicationUser.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower())).FirstOrDefaultAsync();
                        OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser)
                                                    .Where(o=>o.UserId==user.Id)
                                                    .OrderByDescending(o => o.OrderDate).ToListAsync();
                    }
                    else
                    {
                        if (searchPhone != null)
                        {
                            OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser)
                                                        .Where(u => u.PhoneNumber.Contains(searchPhone))
                                                        .OrderByDescending(o => o.OrderDate).ToListAsync();
                        }
                    }
                }
            }
            else
            {
                // all orders ready for pick up
                OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser).Where(u => u.Status == SD.StatusReady).ToListAsync();
            }

            foreach (OrderHeader item in OrderHeaderList)
                {
                    OrderDetailsViewModel individual = new OrderDetailsViewModel
                    {
                        OrderHeader = item,
                        OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                    };
                    orderListVM.Orders.Add(individual);
                }
            


            var count = orderListVM.Orders.Count;
            orderListVM.Orders = orderListVM.Orders.OrderByDescending(p => p.OrderHeader.Id)
                                 .Skip((productPage - 1) * PageSize)
                                 .Take(PageSize).ToList();

            orderListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = count,
                urlParam = param.ToString()
            };

            return View(orderListVM);
        }

        [Authorize(Roles =SD.FrontDeskUser + ","+ SD.ManagerUser)]
        [HttpPost]
        [ActionName("OrderPickup")]
        public async Task<IActionResult> OrderPickupPost(int orderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(orderId);
            orderHeader.Status = SD.StatusCompleted;
            await _db.SaveChangesAsync();
            await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserId).FirstOrDefault().Email, "Spice - Order Completed " + orderHeader.Id.ToString(), "Order has been completed successfully.");

            return RedirectToAction("OrderPickup", "Order");
        }
    }
}
