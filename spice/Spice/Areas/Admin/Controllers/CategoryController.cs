using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)] // authorization
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;   // dependency injection wont have to keep making objects of our model
        }               // can keep reusing


        //GET 
        public async Task<IActionResult> Index()    // Task<IActionResult> when async (uses _db)
        {
            return View(await _db.Category.ToListAsync());
        }

        //GET - CREATE
        public IActionResult Create()   // async not needed since not passing anything to the view to display
        {
        
            // IActionResult when no async
            
            // if we right click on Create and click add view it will create a view in the right directory
            return View();
        }


        //POST - CREATE
        [HttpPost]  // needed for post
        [ValidateAntiForgeryToken]  // needed for post
        public async Task<IActionResult> Create(Category category)
        {
            if(ModelState.IsValid)
            {
                //if valid
                _db.Category.Add(category);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index)); // return to an action(method in controller)
                                                        // nameof is so you dont mis-spell index
            }
            return View(category);  // return back to the view if not valid
        }


        //GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var category = await _db.Category.FindAsync(id);
            if(category==null)
            {
                return NotFound();
            }
            return View(category);  // returning category to the view so the user can edit
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if(ModelState.IsValid)
            {
                _db.Update(category);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }



        //GET - DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _db.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // since changed the action name from one of the key words like delete
        // and get had to define it w/ ActionName("Delete") 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var category = await _db.Category.FindAsync(id);

            if(category ==null)
            {
                return View();
            }
            _db.Category.Remove(category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET - DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _db.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

    }
}
