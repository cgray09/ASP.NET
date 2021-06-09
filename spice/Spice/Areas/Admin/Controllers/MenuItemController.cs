using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostingEnvironment;   // when ever saving something on the server
                                                                    // we need hosting environment
                                                                    // extracts the root path of our app so
                                                                    // we can save user uploaded images to the
                                                                    // images folder
       
        [BindProperty]  // BindProperty allows us to use MenuItemVM globally w/o having to pass it as an argument
        public MenuItemViewModel MenuItemVM { get; set; }
  
        public MenuItemController(ApplicationDbContext db, IWebHostEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            MenuItemVM = new MenuItemViewModel()
            {
                Category = _db.Category,
                MenuItem = new Models.MenuItem()
            };
        }

        public async  Task<IActionResult> Index()
        {
            var menuItems = await _db.MenuItem.Include(m=>m.Category).Include(m=>m.SubCategory).ToListAsync();
            return View(menuItems);
        }

        //GET - CREATE (needed b/c when we hit create button on menu item index page we want to display all 
        // categories and subcategories as an option for two of the inputs in the form)
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            // isnt provided w/ the post and have to get it b/c we made the subcategory select
            // empty in the form to fill dynamically
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            // "SubCategoryId" is the id and name on the form

            if(!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }

            _db.MenuItem.Add(MenuItemVM.MenuItem);  // automatically creates an id for the image in
                                                    // the db which we will use to name our images
                                                    // since names must be different
            await _db.SaveChangesAsync();

            //Work on the image saving section

            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;     // images

            var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);

            if(files.Count>0)
            {
                //files has been uploaded (uploads final uploading loacation of the image)
                var uploads = Path.Combine(webRootPath, "images");  // webRootPath has all the paths before the images folder
                var extension = Path.GetExtension(files[0].FileName);

                // saves the file to the location on the server and renames it
                using (var filesStream = new FileStream(Path.Combine(uploads,MenuItemVM.MenuItem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension; // in db change the image column w/ the location where the image is saved
            }
            else
            {
                //no file was uploaded, so use default
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);  // get default image
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + ".png");  // at it to images folder
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + ".png";   // upload in db
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        //GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }

            // can use MenuItem item w/o creating the object first b/c already did globally
            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();

            if(MenuItemVM.MenuItem ==null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();
                return View(MenuItemVM);
            }

            //Work on the image saving section

            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            // extract the menu item from the db
            var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);

            if (files.Count > 0)
            {
                //New Image has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);

                //Delete the original file (TrimStart('\\') since already a \ in the string, trim it out)
                var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));

                // since replacing remove first
                if(System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                
                // i think "uploads" is where the files is being saved and
                // "MenuItemVM.MenuItem.Id + extension_new" is the name were
                // giving to the file

                //we will upload the new file
                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension_new), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension_new;
            }

            // update each property in db
            menuItemFromDb.Name = MenuItemVM.MenuItem.Name;
            menuItemFromDb.Description = MenuItemVM.MenuItem.Description;
            menuItemFromDb.Price = MenuItemVM.MenuItem.Price;
            menuItemFromDb.Spicyness = MenuItemVM.MenuItem.Spicyness;
            menuItemFromDb.CategoryId = MenuItemVM.MenuItem.CategoryId;
            menuItemFromDb.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //GET : Details MenuItem
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        //GET : Delete MenuItem
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        //POST Delete MenuItem
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            MenuItem menuItem = await _db.MenuItem.FindAsync(id);

            if (menuItem != null)
            {
                var imagePath = Path.Combine(webRootPath, menuItem.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                _db.MenuItem.Remove(menuItem);
                await _db.SaveChangesAsync();

            }

            return RedirectToAction(nameof(Index));
        }
    }
}
