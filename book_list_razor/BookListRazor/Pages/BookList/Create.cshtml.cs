using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookListRazor.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookListRazor.Pages.BookList
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public CreateModel(ApplicationDbContext db)
        {
            _db = db;
        }

        // BindProperty allows us to use Book on onPost b/c Book is what we pass to onPost from the
        // ui when we create a book
        [BindProperty]
        public Book Book { get; set; }

        public void OnGet()
        {

        }

        // IActionResult b/c we will be redirecting to a new page after we create a book
        public async Task<IActionResult> OnPost()
        {
            // ModelState checks the state of the book in terms of the required fields in the model
            // so if name is empty when creating the book this would fail since name is required
            // in the model
            if (ModelState.IsValid)
            {
                await _db.Book.AddAsync(Book);  // add book to the queue to go to db
                await _db.SaveChangesAsync();   // add book to db
                return RedirectToPage("Index");
            }
            else
            {
                return Page();
            }
        }
    }
}