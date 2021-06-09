using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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



namespace Spice.Controllers
{
    [Area("Customer")]  // have to specific what area this controller is in
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        // action method inside this home controller has its own index page to display. 
        // its index page is located in views home for name convention
        public async Task<IActionResult> Index()
        {
            IndexViewModel IndexVM = new IndexViewModel()
            {
                // assign properties to the object
                MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync(),
                Category = await _db.Category.ToListAsync(),
                Coupon = await _db.Coupon.Where(c => c.IsActive == true).ToListAsync()

            };

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // will be null if user is not logged in
            if(claim!=null)
            {
                var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == claim.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);
                // session name SD.ssShoppingCartCount must be consistent so used constant
            }


            return View(IndexVM);
        }

        [Authorize] // must be logged in
        public async Task<IActionResult> Details(int id)
        {
            var menuItemFromDb = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.Id == id).FirstOrDefaultAsync();

            ShoppingCart cartObj = new ShoppingCart()
            {
                MenuItem = menuItemFromDb,
                MenuItemId = menuItemFromDb.Id
            };

            return View(cartObj);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart CartObject)
        {
            CartObject.Id = 0;
            if(ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = await _db.ShoppingCart.Where(c => c.ApplicationUserId == CartObject.ApplicationUserId
                                                && c.MenuItemId == CartObject.MenuItemId).FirstOrDefaultAsync();
                // if null item not in cart so add new item
                if(cartFromDb==null)
                {
                    await _db.ShoppingCart.AddAsync(CartObject);
                }
                else // item in cart so add another by incrementing
                {
                    cartFromDb.Count = cartFromDb.Count + CartObject.Count;
                }
                await _db.SaveChangesAsync();

                var count = _db.ShoppingCart.Where(c => c.ApplicationUserId == CartObject.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, count); // add count to session

                return RedirectToAction("Index");
            }
            else
            {

                var menuItemFromDb = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.Id == CartObject.MenuItemId).FirstOrDefaultAsync();

                // i believe this shows that only one type of product w/ any count
                // can be in the art at a time
                ShoppingCart cartObj = new ShoppingCart()
                {
                    MenuItem = menuItemFromDb,
                    MenuItemId = menuItemFromDb.Id
                };

                return View(cartObj);
            }
        }




        // action method inside this home controller has its own privacy page to display. 
        // its privacy page is located in views home for name convention
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
