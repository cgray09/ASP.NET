using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookListRazor.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookListRazor.Pages.BookList
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        // ApplicationDbContext db got w/ dependency injection
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;   // injecting the dependency injection from the container into this page
                        // so we dont have to create objects manually like he described in diagram
        }

        public IEnumerable<Book> Books { get; set; }

        // had to use "async Task" since using await
        public async Task OnGet()
        {
            // getting all the books from the db
            Books = await _db.Book.ToListAsync();   // used "ctrl dot" here for async to use entity core
        }

        // post handler since it gets called from a button on the ui and Delete
        // since thats the name of the handler on the ui
        public async Task<IActionResult> OnPostDelete(int id)
        {
            var book = await _db.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            _db.Book.Remove(book);
            await _db.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}